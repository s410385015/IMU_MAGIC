#pragma strict

var fadeIn : boolean = true; //是否淡入
var fadeInDuration : float = 1; //淡入時間
@Range(0f,1f) var fadeInVolume : float = 1; //淡入目標音量
var fadeOut : boolean = true; //是否淡出
var fadeOutDuration : float =1;//淡出時間
@Range(0f,1f) var fadeOutVolume : float = 0;//淡出目標音量

private var au : AudioSource;

function Awake(){
    au = GetComponent.<AudioSource>();
    //Debug
    if(au==null){
        Debug.Log("找不到AudioSource元件");
        this.enabled = false;}}

function Update(){
    if(au.isPlaying){
        //淡入
        if(fadeIn && (au.time <= fadeInDuration+0.1)){
            au.volume = fadeInVolume*(au.time/fadeInDuration);
        }
        //淡出
        if(fadeOut && (au.time>=(au.clip.length-fadeOutDuration))){
            au.volume = (fadeInVolume-fadeOutVolume)*((au.clip.length-au.time)/fadeOutDuration)+fadeOutVolume;
        }}}
            



