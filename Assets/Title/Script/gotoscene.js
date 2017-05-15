#pragma strict

import UnityEngine.SceneManagement;

var sceneName : String = "";
var sceneID : int = 0;

function GoToScene(){
if(sceneName ==""){
SceneManager.LoadScene(sceneID);}
else{
SceneManager.LoadScene(sceneName);}}