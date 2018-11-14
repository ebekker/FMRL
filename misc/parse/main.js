// JavaScript source code

Parse.Cloud.define("hello", function (request, response) {
    response.success("Hello world!");
});

Parse.Cloud.define("get", function (request, response) {
    const keyHash = request.params.key_hash;
    const query = new Parse.Query("Message");
    query.equalTo("keyHash", keyHash);
    query.find({ useMasterKey: true }).then((results) => {
        if (results.length > 0) {
            const msg = results[0];
            response.success({
                iv: msg.get("iv"),
                digested: msg.get("digested"),
                encoded: msg.get("encoded"),
                passwordUsed: msg.get("passwordUsed")
            });
        }
        else
            response.error("not found (" + keyHash + ")");
    }, (error) => {
        response.error("not found: " + error);
    });
});

Parse.Cloud.define("del", function (request, response) {
    const keyHash = request.params.key_hash;
    const query = new Parse.Query("Message");
    query.equalTo("keyHash", keyHash);
    query.find({ useMasterKey: true }).then((results) => {
        if (results.length > 0) {
            results[0].destroy().then(() => {
                response.success("deleted");
            }, (err) => {
                response.error("failed to delete: " + error);
            });
        }
        else
            response.error("not found");
    }, (error) => {
        response.error("not found: " + error);
    });
});
