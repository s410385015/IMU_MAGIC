#pragma strict

class Worldtitle extends World{
    var btns : GameObject;
    var anyKeyText : GameObject;

    function Update(){
        if(Input.anyKey){
            btns.SetActive(true);
            anyKeyText.SetActive(false);
         
        }
    }
}