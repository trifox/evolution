mergeInto(LibraryManager.library, {

    OpenFile: function (gameObjectNamePtr, callbackNamePtr) {

        var gameObjectName = Pointer_stringify(gameObjectNamePtr)
        var callbackName = Pointer_stringify(callbackNamePtr)

        var inputId = 'evolution:OpenSimulationWebGL:input:file'

        var fileInput = document.getElementById(inputId)
        if (fileInput) {
            document.body.removeChild(fileInput)
        }

        fileInput = document.createElement('input')
        fileInput.id = inputId
        fileInput.type = 'file'
        fileInput.style.cssText = 'display: none; visibility: hidden;'
        fileInput.accept = '.txt, .evol'

        
        fileInput.onchange = function (e) {
            var file = event.target.files[0]
            var url = URL.createObjectURL(file)
            SendMessage(gameObjectName, callbackName, url)
            URL.revokeObjectURL(url)
            document.body.removeChild(fileInput)
        }

        document.body.appendChild(fileInput)

        document.onmouseup = function () {
            fileInput.click()
            document.onmouseup = null
        }
    }
});