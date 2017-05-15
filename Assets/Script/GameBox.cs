using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBox  {
	private int mode;
	public float Score;
	public int Music;
	public int Combo;
	public int Fail;
	public int Suc;
	public int Perfect;
	public int Good;
	public int Level;
	public int HighestCombo;
	private const int f = 100;
	public int noteID;
	public int noteAngle;
	public int noteDiff;
	private const int AngleDiff = 3;
	private int Angle_prob=15;	
	private int ID_prob=10;

	public GameBox()
	{
		Score = 0;
		Combo = 0;
		Fail = 0;
		Suc = 0;
		Perfect = 0;
		Good = 0;
		HighestCombo = 0;
		noteID=1;
		noteAngle=1;
	}
	public void SetDiff(int d)
	{
		noteDiff = d;
	}
	public void SetMusic(int m)
	{
		Music = m;
		LoadMusic ();
	}
	public void LoadMusic()
	{
		
	}
	public float GetPer(int n)
	{
		if (n == 0)
			return 20.0f;
		else
			return 70.8f;
	}
	public bool isFail()
	{
		return false;
	}
	public void Hit(int n)
	{
		if (n <= 0) {
			Fail++;
			if (Combo > HighestCombo)
				HighestCombo = Combo;
			Combo = 0;
		} else if (n > 0) 
		{
			if (n == 1) {
				Perfect++;
				AddScore (0.25f);
			} else {
				Good++;
				AddScore (0.2f);
			}
			Combo++;
			Suc++;

		}
	}

	private bool ProbMachine(int p)
	{
		Random.seed = System.Guid.NewGuid ().GetHashCode ();
		if (Random.Range (0, 100) > p) {
			return true;
		} else {
			return false;
		}

	}
	public void SetMode(int m)
	{
		
	}
	public void GenerateNote()
	{
		if (ProbMachine (Angle_prob)) {
			int temp = Random.Range (1, AngleDiff);
			if (temp == noteAngle)
				noteAngle = AngleDiff;
			else
				noteAngle = temp;
		} 
		if(ProbMachine(ID_prob))
		{
			int temp = Random.Range (1, noteDiff+1);
			if (temp == noteID)
				noteID = noteDiff;
			else
				noteID = temp;
		}
	}
	public void AddScore(float s)
	{
		//Score+=(s*f*((Combo/10)*0.5f+1));
	}

	public int CheckDir(int n,float x,float y,float z)
	{
		if(n==0)
			return RightHand(x,y,z);
		else if(n==1)
			return LeftHand(x,y,z);
		else if(n==2)
			return RightFoot(x,y,z);
		else if(n==3)
			return LeftFoot(x,y,z);
		else return 0;
	}


	//1上、2右、3左
	private int LeftHand(float x,float y,float z)
	{
		if (z > 30 && z < 150 && y < 30 && y > -30)
			return 2;
		else if (z > 210 && z < 330 && y < 30 && y > -30)
			return 3;
		else if (y < -35)
			return 1;
		else
			return 0;
	}
	private int RightHand(float x,float y,float z)
	{
		if (z > 30 && z < 150 && y < 30 && y > -30)
			return 3;
		else if (z > 210 && z < 330 && y < 30 && y > -30)
			return 2;
		else if (y > 35)
			return 1;
		else
			return 0;
	}
	private int LeftFoot(float x,float y,float z)
	{
		return 0;
	}

	private int RightFoot(float x,float y,float z)
	{
		if (Mathf.Abs (y) < 35) {
			if (z >= 90 && z < 135) {
				return 2;
			} else if (z >= 135 && z < 180) {
				return 1;
			} else if (z >= 180 && z < 225) {
				return 3;
			}
		}
		return 0;
	}
}
