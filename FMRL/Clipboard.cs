using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL
{
    public static class Clipboard
    {
        /// <summary>
        /// Wires up a trigger element (e.g. a button) to a target element (e.g. a text field) to
        /// select and copy the content of the target element when the trigger element is engaged. 
        /// </summary>
        /// <remarks>
        /// Because of the restrictions on using the <c>execCommand("copy")</c> call in some
        /// browsers, that is, they must be invoked inside a <i>short running user-generated
        /// event handler</i>, this routine handles the logic to wire up a <i>copy</i> button
        /// that will select and copy the text of an associated target text field.  This
        /// behavior should work in most situations, and in most browsers.
        /// </remarks>
        public static async Task ConnectTriggerToTarget(ElementRef target, ElementRef trigger)
        {
            await JSRuntime.Current.InvokeAsync<int>(
                "_fmrl.enableCopyButton", target, trigger);
        }
    }
}
