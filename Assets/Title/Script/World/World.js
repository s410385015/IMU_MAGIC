#pragma strict

var sound : SoundManager;


function Awake(){
    sound = FindObjectOfType.<SoundManager>();
    //場景上任何物件上有這個component就把它抓進來
}