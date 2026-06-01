mergeInto(LibraryManager.library, {
    WS_Speak: function (textPtr, pitch, rate) {
        var text = UTF8ToString(textPtr);
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
            
            var utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = 'ar-SA';
            utterance.pitch = pitch;
            utterance.rate = rate;
            
            var voices = window.speechSynthesis.getVoices();
            var arabicVoice = null;
            var preferredVoiceName = 'kore';
            var preferredLangs = ['ar-SA', 'ar_SA'];

            for (var i = 0; i < voices.length; i++) {
                if (voices[i].name && voices[i].name.toLowerCase().indexOf(preferredVoiceName) !== -1) {
                    arabicVoice = voices[i];
                    break;
                }

                if (preferredLangs.indexOf(voices[i].lang) !== -1) {
                    arabicVoice = voices[i];
                    break;
                }

                if (arabicVoice === null && voices[i].lang.indexOf('ar') === 0) {
                    arabicVoice = voices[i];
                }
            }

            if (arabicVoice !== null) {
                utterance.voice = arabicVoice;
            }
            
            window.speechSynthesis.speak(utterance);
        } else {
            console.warn("Speech synthesis not supported in this browser.");
        }
    },
    WS_Stop: function () {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
        }
    }
});
