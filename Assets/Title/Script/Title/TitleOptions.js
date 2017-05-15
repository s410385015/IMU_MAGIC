#pragma strict

var hand : Transform;
var buttons : Transform[];
var xOffest : float[];
var world : World;
var moveSound : AudioClip;
var okSound : AudioClip;

private var id: int =0; //決定選項是哪個

function Start () {
   world = FindObjectOfType.<World>();
    id=0; //從Start Game y位置 開始指
    UpdateHandPosition();
}

function Update () {
    if(Input.GetKeyDown(KeyCode.UpArrow)){//往上選
        id--;
        id = Mathf.Clamp(id,0,3);//id長度固定在0~3之間
        world.sound.PlaySE(moveSound); //增加選擇的音效 
        UpdateHandPosition();//箭頭更新位置
    }
    if(Input.GetKeyDown(KeyCode.DownArrow)){//往下選
        id++;
        id = Mathf.Clamp(id,0,3);//id長度固定在0~3之間
        world.sound.PlaySE(moveSound); //增加選擇的音效 
        UpdateHandPosition();//箭頭更新位置
    }
    //確定選項
    if(Input.GetKeyDown(KeyCode.E)||Input.GetKey("enter")){
        switch(id){
            //Start Game
            case 0:
  				print("?");
                //Game.screen().FadeAndGo("Start"); //跳到StartGame的場景
                Application.LoadLevel(1);
                //this.enable =false;//把程式關起來
            
                break;
            //Ranking
            case 1:
                /* Game.screen().FadeAndGo("Ranking"); //跳到Ranking的場景
                this.enable =false;//把程式關起來*/
                break;
            //Settings
            case 2:
                /*Game.screen().FadeAndGo("Settings"); //跳到Settings的場景
                this.enable =false;//把程式關起來*/
                break;
            //Exit
            case 3:
                Application.Quit();
                break;
        }
        world.sound.PlaySE(okSound);//加上選擇音效 
    }
}

function UpdateHandPosition(){
    hand.position.y =  buttons[id].position.y-0.22;//箭頭的y值=四個按鈕的y值//微調
    hand.position.x = xOffest[id]+1;
}
