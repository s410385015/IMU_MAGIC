#pragma strict

var bgm : AudioSource ;
var se : AudioSource ;

//給一個AudioClip 自動撥se
function PlaySE(au:AudioClip){
    se.PlayOneShot(au);
}