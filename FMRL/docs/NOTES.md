# NOTES

## One-Time Messages (OTM)

##### General procedures:

* A new 128 bit AES key (AesKey) + random IV (AesIV) is generated
* The message content are encrypted using CBC mode and PKCS7 padding (ContentEnc)
* The SHA-256 hash of the AES key is computed (AesKeyHash)
* The SHA-256 hash of the message content is computed (ContentHash)
* The key is encoded in Base32 (AesKeyB32) to save space and create more human friendly form
* The encrypted message is persisted to `/messages/B64U(AesKeyHash)` as:

```JSON
{
    "IV": B64U(AesIV)),
    "Encoded": B64U(ContentEnc),
    "Digested": B64U(ContentHash)
}
```

* The Base32 encoded form of the AES key (in the form of a URL) is sent to the message recipient.

##### Properties:

* The secret key is never stored in any form with the message, only
  the recipient has it so even the "system" can't access the message.

## One-Time Message with Password (OTM+PBK)

##### General procedures:

* The client supplies a string password (Password), which is converted to a
  256-bit AES key (AesPbk) + random salt (AesPbkSalt) + random IV (AesPbkIV)
* The message content are first encrypted using CBC mode and PKCS7 padding (ContentOrigEnc)
* The SHA-256 hash of the message content is computed (ContentOrigHash)
* A new message is generated (ContentWrap) that contains:

```JSON
{
    "pbSalt": B64U(AesPbkSalt)
    "pbIv": B64U(AesPbkIV)
    "pbEncoded": B64U(ContentOrigEnc)
    "pbDigested": B64U(ContentOrigHash)
}
```

* We then process the new message using the simple OTM method, and produce a similar encrypted
  message to persisted to `/messages/B64U(AesKeyHash)`:

```JSON
{
    "IV": B64U(AesIV)),
    "Encoded": B64U(ContentWrapEnc),
    "Digested": B64U(ContentWrapHash),
    "PasswordUsed": true
}
```

* Both the Base32 encoded form of the AES key (in the form of a URL), as well as the original
  client password are distributed to the target recipient, preferably using 2 independent channels

##### Properties:

* In addition to OTM method, we add an addtional

## Shared Secret Message

### Splitting the Shared Secret

* Generate encrypted message as in OTM procedure.
* The 128-bit AES key produced from the OTM procedure (AesKey) is then split-encoded using
  Sharmir's Secret Sharing to produce multiple key shares (SecretShare1, SecretShare2, ...),
  the count equal to the number of requested shares (ShareCount, 2-255) and with the requested
  threshold needed to recover the AES key (ShareThreshold)
* A random mask (SharedMask) is generated equal to the length of each secret share.  Each secret
  share is XORed with this mask to produce the masked secret shares (MaskedSecretShare1, ...);
  the mask is used to ensure the decrypted shares are not stored and made known to the system
* The SHA-256 hash of the mask is computed (SharedMaskHash)
* The shared secret details (SharedSecretDetails) are stored in the ***details area*** at
  **`/shared/details/B64U(SharedMask)`** as:

```JSON
{
    "ShareCount": ShareCount,
    "ShareThreshold": ShareThreshold,
    "B64U(MaskedSecretShare1)": true
    "B64U(MaskedSecretShare2)": true
    "B64U(MaskedSecretShareN)": true
}
```

* Each Shared Secret + Shared Mask is encoded in Base32 and distributed to N recipients (N = ShareCount)

### Recovering the Shared Secret

* A recipient computes the masked form of their share (MaskedSecretShareN) and writes it to a
  ***staging area*** **`/shared/staging/B64U(SharedMask)/B64U(MaskedSecretShareN)`**.
  * The write is only allowed if the same value exists inside the SharedSecretPayload.
  * At least N number of recipients need to write to the staging area where N = ShareThreshold

```JSON
{
    "B64U(MaskedSecretSharing1)": now
    "B64U(MaskedSecretSharing2)": now
    "B64U(MaskedSecretSharingN)": now
}
```

* A client would also monitor the staging area for changes.  As other clients write their
  shared secret to the staging area, all clients attempt to read from the SharedSecretPayload.
* The system only allows reading from SharedSecretPayload when the number of entries in the
  staging area is at least ShareThreshold, then all listening clients can access the secret
  simultaneously.  Then they unmask all the other shares and use Shamir's Secret Sharing to
  recover the AES key.  From there they can recover the secret message as per the OTM method.

CAN'T BE DONE -- Firebase rules does now allow us to express some of these restrictions:

    "shared": {
      "details": {
        "$shared_mask": {
        	// a new message can be created if it does not
          // exist, but it cannot be modified or deleted
          ".write": "!data.exists() && newData.hasChildren(['shareCount', 'shareThreshold', 'maskedShares'])",
          ".read": "data.parent().parent().child('staging').child($shared_mask).val().length >= data.child('shareThreshold').val()",
          "shareCount": { ".validate": "newData.isNumber() && newData.val() < 256"},
          "shareThreshold": { ".validate": "newData.isNumber() && newData.val() > 1 && newData.val() < newData.parent().child('shareCount').val()"},
          "maskedShares": { ".validate": "newData." }// == newData.parent().child('shareCount').val()" }
        }
      },
      "staging": {
        "$shared_mask": {
          "$masked_shared_key": {
            ".read": false,
            ".write": "!newData.exists() && data.parent().parent().parent().child('details').child($shared_mask).hasChild($masked_shared_key)"
          }
        }
      }
    },
