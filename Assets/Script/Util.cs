using System;
//using PInvokeSerialPort;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.ComponentModel;


namespace AssemblyCSharp
{
	public class Gobal
	{
		public static int CAL = 1000;	
	};
	public class Connector
	{
		
		enum State
		{
			CALIBRATION= 0,
			READ_DATA
		};
		enum GLOVE_CMD
		{
			_STOP_= 0,
			_START_,
			_MAG_CAL_,
		};
		public int id;
		private _IMU myIMU;
		private ConcurrentQueue<byte> RxQueue;
		private BLE_WIN32.Win32Com device=new BLE_WIN32.Win32Com();
		private byte[] cmd = { 0x55, 0x00, 0x03, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xAA };
		public bool isConnect=false;
		private string SerialPort="?";
		private int baudrate;
		private bool receiving=false;
		private Thread RxThread;
		private int PacketSize=26;
		public int processed = 0;
		private int state = (int)State.CALIBRATION;
		private Byte[] tempArray;
		public float[] acc = new float[3];
		public float[] gyro = new float[3];
		public float[] mag = new float[3];
		private byte[] AccRaw = new byte[6];
		private byte[] GyroRaw = new byte[6];
		private byte[] MagRaw = new byte[6];
		private byte[] TimeRaw = new byte[5];
		private float[] AccOffset = new float[3];
		private float[] GyroOffset = new float[3];
		private float[] MagOffset = new float[3];
		public float[] euler = new float[3];
		public float[] q = new float[4];
		private BackgroundWorker ReceiveProcess = new BackgroundWorker();
		private Semaphore ReceiveEndSync;

		public Connector()
		{

		}

		public bool SetUp(string COM,int br,int num)
		{
			myIMU = new _IMU ();
			RxQueue = new ConcurrentQueue<byte> ();
			ReceiveEndSync = new Semaphore(0, 1);
			ReceiveProcess.WorkerReportsProgress = true;
			ReceiveProcess.WorkerSupportsCancellation = true;
			ReceiveProcess.DoWork += new DoWorkEventHandler(Parse);
			ReceiveProcess.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_ReceiveProcessRunWorkerCompleted);

			tempArray = new Byte[512];
			id=num;
			SerialPort=COM;
			baudrate=br;

			isConnect = false;
			device.CloseCom();
			if(device.OpenCom(COM,br))
			{

				if(CmdWrite((byte)GLOVE_CMD._STOP_))
				{
					if(CmdWrite((byte)GLOVE_CMD._START_))
					{
						receiving=true;
						RxThread=new Thread(DoReceive);
						RxThread.IsBackground=true;
						RxThread.Start();
						ReceiveProcess.RunWorkerAsync();
						isConnect = true;
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
	
			if (!isConnect) {
				device.CloseCom ();
				return false;
			}
			return false;

		}
		public void close()
		{
			
			RxThread.Abort();
			ReceiveProcess.CancelAsync();
			if (CmdWrite((byte)GLOVE_CMD._STOP_))
			{
				
				receiving = false;
				isConnect = false;
				device.CloseCom();
			
			}
			receiving = false;
			isConnect = false;
			device.CloseCom();
		}
		private bool CmdWrite(byte bCmd)
		{
			bool wRes=false;
			byte[] resbuf=new byte[11];
			cmd[1]=bCmd;
			if(device.WriteCom(cmd,11))
			{

				bCmd &= 0xF;
				Thread.Sleep(100);
				if (bCmd == (byte)GLOVE_CMD._STOP_)
				{
					if (device.ReadCom(resbuf, 11) != 0)
					{
						
						wRes = true;
					}

				}
				else if (bCmd == (byte)GLOVE_CMD._START_)
				{

					myIMU.SetRes(cmd[2], cmd[3], cmd[4]);

					wRes = true;
				}
				else if (bCmd == (byte)GLOVE_CMD._MAG_CAL_)
				{
					bool bReady = false;
					Thread.Sleep(100);

					if (!bReady)
					{
						if (device.ReadCom(resbuf, 11) != 0)
						{
							bReady = true;
						}
					}
					wRes = true;
				}
			}
			return wRes;
		}
		private void DoReceive()
		{
			int ContentLength = 0;
			int index = 0;

			while (receiving)
			{

				byte[] bBuf = new byte[2048000];
				int length = 0;
				length = device.ReadCom(bBuf, 2048000);

				if (length > 0)
				{
					ContentLength += length;
					lock (RxQueue)
					{
						for (int j = 0; j < length; j++)
							RxQueue.Enqueue(bBuf[j]);
						index += length;
					}
				}
			}
		}
		private int IdxOfHeader(byte[] bArray)
		{
			int iRes = -1;
			for (int i = 0; i < bArray.Length; i++)
			{
				if ((0x55 == bArray[i]) && (0xAA == bArray[i + 1]))
				{
					iRes = i;
					break;
				}
			}
			return iRes;
		}
		private bool RxDataParse()
		{
			bool bRes = false;
			int Length = 0;
			byte[] array;
			lock (RxQueue)
			{
				Length = RxQueue.Count;
				array = new byte[Length];
				int iIdx;

				if (Length >= PacketSize)
				{
					array=RxQueue.CopyTo();   

					iIdx = IdxOfHeader(array);

					if ((iIdx >= 0) && (Length - (iIdx + PacketSize) > 2))
					{
						if ((array[iIdx + PacketSize] == 0x55) && (array[iIdx + PacketSize + 1] == 0xAA))
						{

							DeQueProc(iIdx);
							GetQueDataProc(PacketSize,ref tempArray);

							for (int index = 0; index < 6; index++)
							{
								AccRaw[index] = tempArray[8+index];
								GyroRaw[index] = tempArray[14+index];
								MagRaw[index] = tempArray[20 + index];
							}

							for (int i = 0; i < 5; i++)
								TimeRaw [i] = tempArray [2 + i];
							Array.Clear(tempArray, 0, tempArray.Length);
							bRes = true;
						}
						else
						{
							DeQueProc(iIdx + 2);
						}
					}
				}
			}
			return bRes;
		}

		private void Parse(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker ReceiveWorker = sender as BackgroundWorker;

			while (receiving)
			{
		
				if (ReceiveWorker.CancellationPending == true)
				{
					e.Cancel = true;
					ReceiveEndSync.Release();
					break;
				}
				else
				{

					if (RxDataParse())
					{
						IMU_StateMachine();
						ReceiveWorker.ReportProgress(processed);
						//if (processed == 75000)
							//print ("Close");
						Thread.Sleep(1);
					}
				}
			}
		}
		private void DeQueProc(int iCount)
		{
			byte get;
			for (int i = 0; i < iCount; i++)
			{
				RxQueue.TryDequeue(out get);
			}
		}
		private void GetQueDataProc(int iCount,ref byte[] bArray)
		{
			for (int i = 0; i < iCount; i++)
			{
				RxQueue.TryDequeue(out bArray[i]);
			}
		}
		private void IMU_StateMachine()
		{
			switch (state)
			{
			case (int)State.CALIBRATION:

				myIMU.SetData(id, AccRaw, GyroRaw, MagRaw, TimeRaw);
				myIMU.UpdateAGOffset();
				myIMU.UpdateMOffset();

				processed++;
				if (processed == Gobal.CAL)
				{

					AccOffset = myIMU.GetAccOffset();
					GyroOffset = myIMU.GetGyroOffset();
					MagOffset = myIMU.GetMagOffset();
					myIMU.bCalbration = true;
					processed = 0;

					state = (int)State.READ_DATA;                        
				}
				break;
			case (int)State.READ_DATA:
				
				myIMU.SetData (id, AccRaw, GyroRaw, MagRaw, TimeRaw);
		
				myIMU.update ();
				acc = myIMU.GetAcc ();
				gyro = myIMU.GetGyro();
				mag = myIMU.GetMag();
				euler = myIMU.GetEuler();
				q = myIMU.GetQuaternion();



				processed++;
				break;
			default:
				break;

			}
		}
		private void bw_ReceiveProcessRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}
	}
	/*
	public class Connector
	{
		private int ID;
		public SerialPort Device;
		private byte[] cmd = { 0x55, 0x01, 0x01, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xAA };
		private const int package_length = 29;
		public int byte_rec;
		public int MaxLength = 26;
		public byte[] data;
		public _IMU myIMU;
		public Quaternion _q;
		public float[] e = { 0f, 0f, 0f };
		public bool Isopen=false;

		public Connector()
		{
			
		}

		public void UpdateQ()
		{
			myIMU.UpdateAGOffset ();
			myIMU.update ();
			float[] q = new float[4];
			q = myIMU.GetQuaternion ();
			e = myIMU.GetEuler ();
			_q=new Quaternion (q[0],q[1],q[2],q[3]);

		}

		public void Set(string COM,int Baudrate,int num)
		{
			ID = num;
			Device = new SerialPort(COM, Baudrate);
			byte_rec = 0;
			data = new byte[MaxLength];
			myIMU = new _IMU ();
		}
		public void InsertData(byte x)
		{
			data [byte_rec] = x;
		}
		public bool SetUp()
		{
			try{
				Device.Open ();
				Device.Write (cmd);
				Isopen=true;
				return true;
			}
			catch {
				return false;//print ("error");
			}


		}
	};
	*/
	public class TIME
	{
		private int _hour = 0;
		private int _minute = 0;
		private int _second = 0;
		private int _millisecond = 0;
		private int TotalTime = 0;

		public void SetTIME(int hour, int minute, int second, int milli_H, int milli_L)
		{
			_hour = hour;
			_minute = minute;
			_second = second;
			_millisecond = milli_H << 8 | milli_L;
			TotalTime = _hour * 60 * 60 * 1000 + _minute * 60 * 1000 + _second * 1000 + _millisecond;
		}

		public int GetTotalTime()
		{
			return TotalTime;
		}

	};


    public class _IMU
    {
        private DateTime Pre_Time;//, Now_Time;
        //data of IMU
        private int _id;
        private float[] _acc;
        private float[] _gyro;
        private float[] _mag;
        private TIME _time;
        private int past_time;
        private int delt = 1;//, delt_cal;
        //offset of IMU
        private float[] AccTotal = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] GyroTotal = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] AccOffset = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] GyroOffset = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] MagOffset = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] MagScaleDest = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] MagMax = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] MagMin = new float[3] { 0.0f, 0.0f, 0.0f };
        //Scale of IMU
        private int gScale;
        private int aScale;
        private int mScale;

        private float gRes;
        private float aRes;
        private float mRes;
        
        //attitude
        private float[] q;
        private float[] angle;
        private MadgwickAHRS filter;

     
        private int processed = 0;
        public bool bCalbration = false;

        // gyro_scale defines the possible full-scale ranges of the gyroscope:
        enum gyro_scale
        {
            G_SCALE_250DPS = 0,		// 00:  245 degrees per second
            G_SCALE_500DPS,		// 01:  500 dps
            G_SCALE_1000DPS,	// 10:  2000 dps
            G_SCALE_2000DPS	// 11:  2000 dps
        };

        // accel_scale defines all possible FSR's of the accelerometer:
        enum accel_scale
        {
            A_SCALE_2G = 0,	// 000:  2g
            A_SCALE_4G,	// 001:  4g
            A_SCALE_8G,	// 011:  8g
            A_SCALE_16G	// 100:  16g
        };
        // mag_scale defines all possible FSR's of the magnetometer:
        enum mag_scale
        {
            M_SCALE_14BIT = 0,	// 10:  8Gs
            M_SCALE_16BIT,	// 11:  12Gs
        };

        public _IMU()
        {
            Pre_Time = DateTime.Now;
            _id = 0;
            _time = new TIME();
            _acc = new float[3] { 0.0f, 0.0f, 0.0f };
            _gyro = new float[3] { 0.0f, 0.0f, 0.0f };
            _mag = new float[3] { 0.0f, 0.0f, 0.0f };

            AccOffset = new float[3] { 0.0f, 0.0f, 0.0f };
            GyroOffset = new float[3] { 0.0f, 0.0f, 0.0f };
            MagOffset = new float[3] { 0.0f, 0.0f, 0.0f };
            
            q = new float[4] { 1.0f, 0.0f, 0.0f, 0.0f };
            
            angle = new float[3] { 0.0f, 0.0f, 0.0f };
            
            filter = new MadgwickAHRS(17f / 1000f, 0.5f);
            
            gScale = (int)gyro_scale.G_SCALE_250DPS;
            aScale = (int)accel_scale.A_SCALE_2G;
            mScale = (int)mag_scale.M_SCALE_14BIT;
            
            calcgRes();
            calcaRes();
            calcmRes();
        }
        public void SetRes(int iAccScale, int iGyroScale, int iMagScale)
        {
            aScale = iAccScale;
            gScale = iGyroScale;
            mScale = iMagScale;

            calcgRes();
            calcaRes();
            calcmRes();
        }
        public void update()
        {

            if (CheckMag())
            {
				//Debug.Log (_acc [0] + " " + _acc[1] + " " + _acc [2] + " ");
                filter.Update(deg2rad(_gyro[0]), deg2rad(_gyro[1]), deg2rad(_gyro[2]), _acc[0], _acc[1], _acc[2], _mag[0], _mag[1], _mag[2], delt / 1000.0f);
                //filter.Update(deg2rad(_gyro[0]), deg2rad(_gyro[1]), deg2rad(_gyro[2]), _acc[0], _acc[1], _acc[2], delt / 1000.0f);
            }
            else
            {
                filter.Update(deg2rad(_gyro[0]), deg2rad(_gyro[1]), deg2rad(_gyro[2]), _acc[0], _acc[1], _acc[2], delt / 1000.0f);
            }
		
            q = filter.Quaternion;
			for (int i = 0; i < 3; i++)
                this.angle[i] = filter.euler[i];
			
        }

        public void SetData(int id, byte[] acc, byte[] gyro, byte[] mag, byte[] time)
        {
             
            _id = id;
            CalcAcc(acc);
            CalcGyro(gyro);
            CalcMag(mag);
            _time.SetTIME(time[0], time[1], time[2], time[3], time[4]);
            CalcDelt();

        }

        public int GetTime()
        {
            return _time.GetTotalTime();
        }

        public int GetDeltaTime()
        {
            return delt;
        }

        public void SetOffset(float[] accoffset, float[] gyrooffset, float[] magoffset)
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    this.AccOffset[i] = _acc[i];
            //    this.GyroOffset[i] = _gyro[i];
            //    this.MagOffset[i] = _mag[i];
            //}
            for (int i = 0; i < 3; i++)
            {
                this._acc[i] = AccOffset[i];
                this._gyro[i] = GyroOffset[i];
                this._mag[i] = MagOffset[i];
            }
        }

        public float[] GetAccOffset()
        {
            return AccOffset;
        }

        public float[] GetGyroOffset()
        {
            return GyroOffset;
        }

        public float[] GetMagOffset()
        {
            return MagOffset;
        }

        public float[] GetAcc()
        {
            return this._acc;
        }

        public float[] GetGyro()
        {
            return this._gyro;
        }
        
        public float[] GetMag()
        {
            return this._mag;
        }

        public float[] GetQuaternion()
        {
            return this.q;
        }

        public float[] GetEuler()
        {
            return this.angle;
        }

        //Calculate the value of glove
        private void CalcAcc(byte[] acc)
        {
            for (int index = 0; index < 3; index++)
            {
                this._acc[index] = (float)((short)((acc[2 * index] << 8) | (acc[2 * index + 1] & 0xff)) * aRes);
                //if (bCalbration)
                //{
                //    this._acc[index] = this._acc[index] - this.AccOffset[index];
                //}
            }
        }

        private void CalcGyro(byte[] gyro)
        {
            for (int index = 0; index < 3; index++)
            {
                this._gyro[index] = (float)((short)((gyro[2 * index] << 8) | (gyro[2 * index + 1] & 0xff)) * gRes);
                //if (bCalbration)
                //{
                //    this._gyro[index] = this._gyro[index] - this.GyroOffset[index];
                //}
            }
        }

        private void CalcMag(byte[] mag)
        {
            for (int index = 0; index < 3; index++)
            {
                this._mag[index] = (float)(((short)((mag[2 * index] << 8) | (mag[2 * index + 1] & 0xff)) * mRes) / 1000.0f);

                if (bCalbration)
                {
                    this._mag[index] = (this._mag[index] - this.MagOffset[index]) *this.MagScaleDest[index];
                }
            }
        }

        //calculate gyro Resolution
        private void calcgRes()
        {
            // Possible gyro scales (and their register bit settings) are:
            // 245 DPS (00), 500 DPS (01), 2000 DPS (10). Here's a bit of an algorithm
            // to calculate DPS/(ADC tick) based on that 2-bit value:
            
            switch (gScale)
            {
            case (int)gyro_scale.G_SCALE_250DPS:
                gRes = 250.0f / 32768.0f;
            break;
            case (int)gyro_scale.G_SCALE_500DPS:
                gRes = 500.0f / 32768.0f;
            break;
            case (int)gyro_scale.G_SCALE_1000DPS:
                gRes = 1000.0f / 32768.0f;
            break;
            case (int)gyro_scale.G_SCALE_2000DPS:
                gRes = 2000.0f / 32768.0f;
            break;
            }
        }

        //calculate acc Resolution
        private void calcaRes()
        {
            // Possible accelerometer scales (and their register bit settings) are:
            // 2 g (000), 4g (001), 6g (010) 8g (011), 16g (100). Here's a bit of an 
            // algorithm to calculate g/(ADC tick) based on that 3-bit value:
            switch (aScale)
            {
                case (int)accel_scale.A_SCALE_2G:
                    aRes = 2.0f / 32768.0f;
                    break;
                case (int)accel_scale.A_SCALE_4G:
                    aRes = 4.0f / 32768.0f;
                    break;
                case (int)accel_scale.A_SCALE_8G:
                    aRes = 8.0f / 32768.0f;
                    break;
                case (int)accel_scale.A_SCALE_16G:
                    aRes = 16.0f / 32768.0f;
                    break;
            }
            
                
        }

        //calculate mag Resolution
        private void calcmRes()
        {
            // Possible magnetometer scales (and their register bit settings) are:
            // 2 Gs (00), 4 Gs (01), 8 Gs (10) 12 Gs (11). Here's a bit of an algorithm
            // to calculate Gs/(ADC tick) based on that 2-bit value:
            switch (mScale)
            {
                case (int)mag_scale.M_SCALE_14BIT:
                    mRes = 10.0f * 4912.0f / 8192.0f; // Proper scale to return milliGauss
                break;
                case (int)mag_scale.M_SCALE_16BIT:
                    mRes = 10.0f * 4912.0f / 32767.0f;
                break;
            }
  
        }
        public void UpdateAGOffset()
        {
            for (int index = 0; index < 3; index++)
            {
                AccTotal[index] += _acc[index];
                GyroTotal[index] += _gyro[index];
            }
            processed++;
			if (processed == Gobal.CAL)
            {
                //計算Offset
                for (int index = 0; index < 3; index++)
                {
                    AccOffset[index] = (float)(AccTotal[index] / processed);
                    GyroOffset[index] = (float)(GyroTotal[index] / processed);
                }

                AccOffset[0] = AccOffset[0] - 0.0f;
                AccOffset[1] = AccOffset[1] - 0.0f;
                AccOffset[2] = AccOffset[2] - 1.0f;

                processed = 0;
            }
        }

        public void UpdateMOffset()
        {
            //取代最大最小值
            for (int index = 0; index < 3; index++)
            {
                if (_mag[index] > MagMax[index])
                {
                    MagMax[index] = _mag[index];
                }
                else if (_mag[index] < MagMin[index])
                {
                    MagMin[index] = _mag[index];
                }
            }
            processed++;
			if (processed == Gobal.CAL)
            {
                for (int index = 0; index < 3; index++)
                {
                    MagOffset[index] = (MagMax[index] + MagMin[index]) / 2.0f;
                    MagScaleDest[index] = (MagMax[index] - MagMin[index]) / 2.0f;
                }
                float avg_rad = MagScaleDest[0] + MagScaleDest[1] + MagScaleDest[2];
                avg_rad /= 3.0f;
                for (int index = 0; index < 3; index++)
                {
                    MagScaleDest[index] = avg_rad / MagScaleDest[index];
                }
                processed = 0;
            }
        }

        private void CalcDelt()
        {
            int current_time = _time.GetTotalTime();
            delt = current_time - past_time;
            past_time = current_time;                    
        }

       

        private bool CheckMag()
        {
            if ((MagOffset[0] == 0.0f) && (MagOffset[1] == 0.0f) && (MagOffset[2] == 0.0f))
                return false;
            else
                return true;
        }

        static float deg2rad(float degrees)
        {
            return (float)(Math.PI / 180) * degrees;
        }
    };


    public class MadgwickAHRS
    {
        public float[] euler = new float[3] { 0.0f, 0.0f, 0.0f };
        //private float GyroMeasError = (float)Math.PI * (40.0f / 180.0f);
        //private float GyroMeasDrift = (float)Math.PI * (0.0f / 180.0f);
        float exInt = 0, eyInt = 0, ezInt = 0;
        const float Kp = 2.0f;			// proportional gain governs rate of convergence to accelerometer/magnetometer
        const float Ki = 0.005f;		// integral gain governs rate of convergence of gyroscope biases
        const float halfT = 0.5f * (1f / 100f);		// half the sample period

        /// <summary>
        /// Gets or sets the sample period.
        /// </summary>
        public float SamplePeriod { get; set; }

        /// <summary>
        /// Gets or sets the algorithm gain beta.
        /// </summary>
        public float Beta { get; set; }

        /// <summary>
        /// Gets or sets the Quaternion output.
        /// </summary>
        public float[] Quaternion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        public MadgwickAHRS(float samplePeriod)
            : this(samplePeriod, 1f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        /// <param name="beta">
        /// Algorithm gain beta.
        /// </param>
        public MadgwickAHRS(float samplePeriod, float beta)
        {
            SamplePeriod = samplePeriod;
            //Beta = (float)Math.Sqrt(3.0f / 4.0f) * GyroMeasError;
            Beta = 0.6f;
            Quaternion = new float[] { 1f, 0f, 0f, 0f };
        }

        
        /// <summary>
        /// Algorithm AHRS update method. Requires only gyroscope and accelerometer data.
        /// </summary>
        /// <param name="gx">
        /// Gyroscope x axis measurement in radians/s.
        /// </param>
        /// <param name="gy">
        /// Gyroscope y axis measurement in radians/s.
        /// </param>
        /// <param name="gz">
        /// Gyroscope z axis measurement in radians/s.
        /// </param>
        /// <param name="ax">
        /// Accelerometer x axis measurement in any calibrated units.
        /// </param>
        /// <param name="ay">
        /// Accelerometer y axis measurement in any calibrated units.
        /// </param>
        /// <param name="az">
        /// Accelerometer z axis measurement in any calibrated units.
        /// </param>
        /// <param name="mx">
        /// Magnetometer x axis measurement in any calibrated units.
        /// </param>
        /// <param name="my">
        /// Magnetometer y axis measurement in any calibrated units.
        /// </param>
        /// <param name="mz">
        /// Magnetometer z axis measurement in any calibrated units.
        /// </param>
        /// <remarks>
        /// Optimised for minimal arithmetic.
        /// Total ±: 160
        /// Total *: 172
        /// Total /: 5
        /// Total sqrt: 5
        /// 
        
        /// </remarks> 

        public void AHRSupdate(float gx, float gy, float gz, float ax, float ay, float az, float mx, float my, float mz, float deltat)
        {
            float norm;
            float hx, hy, hz, bx, bz;
            float vx, vy, vz, wx, wy, wz;
            float ex, ey, ez;
            float q0 = Quaternion[0], q1 = Quaternion[1], q2 = Quaternion[2], q3 = Quaternion[3];   // short name local variable for readability


            // auxiliary variables to reduce number of repeated operations
            float q0q0 = q0 * q0;
            float q0q1 = q0 * q1;
            float q0q2 = q0 * q2;
            float q0q3 = q0 * q3;
            float q1q1 = q1 * q1;
            float q1q2 = q1 * q2;
            float q1q3 = q1 * q3;
            float q2q2 = q2 * q2;
            float q2q3 = q2 * q3;
            float q3q3 = q3 * q3;

            // normalise the measurements
            norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            ax = ax / norm;
            ay = ay / norm;
            az = az / norm;
            norm = (float)Math.Sqrt(mx * mx + my * my + mz * mz);
            mx = mx / norm;
            my = my / norm;
            mz = mz / norm;

            // compute reference direction of flux
            hx = (float)(2.0f * mx * (0.5 - q2q2 - q3q3) + 2 * my * (q1q2 - q0q3) + 2 * mz * (q1q3 + q0q2));
            hy = (float)(2.0f * mx * (q1q2 + q0q3) + 2 * my * (0.5 - q1q1 - q3q3) + 2 * mz * (q2q3 - q0q1));
            hz = (float)(2.0f * mx * (q1q3 - q0q2) + 2 * my * (q2q3 + q0q1) + 2 * mz * (0.5 - q1q1 - q2q2));
            bx = (float)Math.Sqrt((hx * hx) + (hy * hy));
            bz = hz;

            // estimated direction of gravity and flux (v and w)
            vx = 2 * (q1q3 - q0q2);
            vy = 2 * (q0q1 + q2q3);
            vz = q0q0 - q1q1 - q2q2 + q3q3;
            wx = (float)(2 * bx * (0.5 - q2q2 - q3q3) + 2 * bz * (q1q3 - q0q2));
            wy = (float)(2 * bx * (q1q2 - q0q3) + 2 * bz * (q0q1 + q2q3));
            wz = (float)(2 * bx * (q0q2 + q1q3) + 2 * bz * (0.5 - q1q1 - q2q2));

            // error is sum of cross product between reference direction of fields and direction measured by sensors
            ex = (ay * vz - az * vy) + (my * wz - mz * wy);
            ey = (az * vx - ax * vz) + (mz * wx - mx * wz);
            ez = (ax * vy - ay * vx) + (mx * wy - my * wx);

            // integral error scaled integral gain
            exInt = exInt + ex * Ki;
            eyInt = eyInt + ey * Ki;
            ezInt = ezInt + ez * Ki;

            // adjusted gyroscope measurements
            gx = gx + Kp * ex + exInt;
            gy = gy + Kp * ey + eyInt;
            gz = gz + Kp * ez + ezInt;

            // integrate quaternion rate and normalise
            q0 = q0 + (-q1 * gx - q2 * gy - q3 * gz) * deltat;
            q1 = q1 + (q0 * gx + q2 * gz - q3 * gy) * deltat;
            q2 = q2 + (q0 * gy - q1 * gz + q3 * gx) * deltat;
            q3 = q3 + (q0 * gz + q1 * gy - q2 * gx) * deltat;

            // normalise quaternion
            norm = (float)Math.Sqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            Quaternion[0] = q0 / norm;
            Quaternion[1] = q1 / norm;
            Quaternion[2] = q2 / norm;
            Quaternion[3] = q3 / norm;
            QuaternionToEuler();
        }
        
        
        
        public void Update(float gx, float gy, float gz, float ax, float ay, float az, float mx, float my, float mz, float deltat)
        {


            float q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
            float norm;
            float hx, hy, _2bx, _2bz;
            float s1, s2, s3, s4;
            float qDot1, qDot2, qDot3, qDot4;

            // Auxiliary variables to avoid repeated arithmetic
            float _2q1mx;
            float _2q1my;
            float _2q1mz;
            float _2q2mx;
            float _4bx;
            float _4bz;
            float _2q1 = 2f * q1;
            float _2q2 = 2f * q2;
            float _2q3 = 2f * q3;
            float _2q4 = 2f * q4;
            float _2q1q3 = 2f * q1 * q3;
            float _2q3q4 = 2f * q3 * q4;
            float q1q1 = q1 * q1;
            float q1q2 = q1 * q2;
            float q1q3 = q1 * q3;
            float q1q4 = q1 * q4;
            float q2q2 = q2 * q2;
            float q2q3 = q2 * q3;
            float q2q4 = q2 * q4;
            float q3q3 = q3 * q3;
            float q3q4 = q3 * q4;
            float q4q4 = q4 * q4;

            // Normalise accelerometer measurement
            norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            if (norm == 0f) return; // handle NaN
            norm = 1 / norm;        // use reciprocal for division
            ax *= norm;
            ay *= norm;
            az *= norm;

            // Normalise magnetometer measurement
            norm = (float)Math.Sqrt(mx * mx + my * my + mz * mz);
            if (norm == 0f) return; // handle NaN
            norm = 1 / norm;        // use reciprocal for division
            mx *= norm;
            my *= norm;
            mz *= norm;

            // Reference direction of Earth's magnetic field
            _2q1mx = 2f * q1 * mx;
            _2q1my = 2f * q1 * my;
            _2q1mz = 2f * q1 * mz;
            _2q2mx = 2f * q2 * mx;
            hx = mx * q1q1 - _2q1my * q4 + _2q1mz * q3 + mx * q2q2 + _2q2 * my * q3 + _2q2 * mz * q4 - mx * q3q3 - mx * q4q4;
            hy = _2q1mx * q4 + my * q1q1 - _2q1mz * q2 + _2q2mx * q3 - my * q2q2 + my * q3q3 + _2q3 * mz * q4 - my * q4q4;
            _2bx = (float)Math.Sqrt(hx * hx + hy * hy);
            _2bz = -_2q1mx * q3 + _2q1my * q2 + mz * q1q1 + _2q2mx * q4 - mz * q2q2 + _2q3 * my * q4 - mz * q3q3 + mz * q4q4;
            _4bx = 2f * _2bx;
            _4bz = 2f * _2bz;

            // Gradient decent algorithm corrective step
            s1 = -_2q3 * (2f * q2q4 - _2q1q3 - ax) + _2q2 * (2f * q1q2 + _2q3q4 - ay) - _2bz * q3 * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q4 + _2bz * q2) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q3 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s2 = _2q4 * (2f * q2q4 - _2q1q3 - ax) + _2q1 * (2f * q1q2 + _2q3q4 - ay) - 4f * q2 * (1 - 2f * q2q2 - 2f * q3q3 - az) + _2bz * q4 * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q3 + _2bz * q1) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q4 - _4bz * q2) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s3 = -_2q1 * (2f * q2q4 - _2q1q3 - ax) + _2q4 * (2f * q1q2 + _2q3q4 - ay) - 4f * q3 * (1 - 2f * q2q2 - 2f * q3q3 - az) + (-_4bx * q3 - _2bz * q1) * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q2 + _2bz * q4) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q1 - _4bz * q3) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            s4 = _2q2 * (2f * q2q4 - _2q1q3 - ax) + _2q3 * (2f * q1q2 + _2q3q4 - ay) + (-_4bx * q4 + _2bz * q2) * (_2bx * (0.5f - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q1 + _2bz * q3) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q2 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5f - q2q2 - q3q3) - mz);
            norm = 1f / (float)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
            s1 *= norm;
            s2 *= norm;
            s3 *= norm;
            s4 *= norm;

            // Compute rate of change of quaternion - step 1
            qDot1 = 0.5f * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
            qDot2 = 0.5f * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
            qDot3 = 0.5f * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
            qDot4 = 0.5f * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;

            // Integrate to yield quaternion - step 2
            q1 += qDot1 * deltat;
            q2 += qDot2 * deltat;
            q3 += qDot3 * deltat;
            q4 += qDot4 * deltat;

            norm = 1f / (float)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
            Quaternion[0] = q1 * norm;
            Quaternion[1] = q2 * norm;
            Quaternion[2] = q3 * norm;
            Quaternion[3] = q4 * norm;

            QuaternionToEuler();
        }

        /// <summary>
        /// Algorithm IMU update method. Requires only gyroscope and accelerometer data.
        /// </summary>
        /// <param name="gx">
        /// Gyroscope x axis measurement in radians/s.
        /// </param>
        /// <param name="gy">
        /// Gyroscope y axis measurement in radians/s.
        /// </param>
        /// <param name="gz">
        /// Gyroscope z axis measurement in radians/s.
        /// </param>
        /// <param name="ax">
        /// Accelerometer x axis measurement in any calibrated units.
        /// </param>
        /// <param name="ay">
        /// Accelerometer y axis measurement in any calibrated units.
        /// </param>
        /// <param name="az">
        /// Accelerometer z axis measurement in any calibrated units.
        /// </param>
        /// <remarks>
        /// Optimised for minimal arithmetic.
        /// Total ±: 45
        /// Total *: 85
        /// Total /: 3
        /// Total sqrt: 3
        /// </remarks>
        public void Update(float gx, float gy, float gz, float ax, float ay, float az, float deltat)
        {
            float q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
            float norm;
            float s1, s2, s3, s4;
            float qDot1, qDot2, qDot3, qDot4;

            // Auxiliary variables to avoid repeated arithmetic
            float _2q1 = 2f * q1;
            float _2q2 = 2f * q2;
            float _2q3 = 2f * q3;
            float _2q4 = 2f * q4;
            float _4q1 = 4f * q1;
            float _4q2 = 4f * q2;
            float _4q3 = 4f * q3;
            float _8q2 = 8f * q2;
            float _8q3 = 8f * q3;
            float q1q1 = q1 * q1;
            float q2q2 = q2 * q2;
            float q3q3 = q3 * q3;
            float q4q4 = q4 * q4;

            // Normalise accelerometer measurement
            norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            if (norm == 0f) return; // handle NaN
            norm = 1 / norm;        // use reciprocal for division
            ax *= norm;
            ay *= norm;
            az *= norm;

            // Gradient decent algorithm corrective step
            s1 = _4q1 * q3q3 + _2q3 * ax + _4q1 * q2q2 - _2q2 * ay;
            s2 = _4q2 * q4q4 - _2q4 * ax + 4f * q1q1 * q2 - _2q1 * ay - _4q2 + _8q2 * q2q2 + _8q2 * q3q3 + _4q2 * az;
            s3 = 4f * q1q1 * q3 + _2q1 * ax + _4q3 * q4q4 - _2q4 * ay - _4q3 + _8q3 * q2q2 + _8q3 * q3q3 + _4q3 * az;
            s4 = 4f * q2q2 * q4 - _2q2 * ax + 4f * q3q3 * q4 - _2q3 * ay;
            norm = 1f / (float)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
            s1 *= norm;
            s2 *= norm;
            s3 *= norm;
            s4 *= norm;

            // Compute rate of change of quaternion
            qDot1 = 0.5f * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
            qDot2 = 0.5f * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
            qDot3 = 0.5f * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
            qDot4 = 0.5f * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;

            // Integrate to yield quaternion
            q1 += qDot1 * deltat;
            q2 += qDot2 * deltat;
            q3 += qDot3 * deltat;
            q4 += qDot4 * deltat;
            norm = 1f / (float)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
            Quaternion[0] = q1 * norm;
            Quaternion[1] = q2 * norm;
            Quaternion[2] = q3 * norm;
            Quaternion[3] = q4 * norm;

            QuaternionToEuler();
        }
        
        public void QuaternionToEuler()
        {
            
                //float yaw   = (float)Math.Atan2(2.0f * (Quaternion[1] * Quaternion[2] + Quaternion[0] * Quaternion[3]), Quaternion[0] * Quaternion[0] + Quaternion[1] * Quaternion[1] - Quaternion[2] * Quaternion[2] - Quaternion[3] * Quaternion[3]);
                //float yaw = 2.0f * (float)Math.Acos(Quaternion[0]);
                //float pitch = (float)-Math.Asin(2.0f * (Quaternion[1] * Quaternion[3] - Quaternion[0] * Quaternion[2]));
                //float roll = (float)Math.Atan2(2.0f * (Quaternion[0] * Quaternion[1] + Quaternion[2] * Quaternion[3]), Quaternion[0] * Quaternion[0] - Quaternion[1] * Quaternion[1] - Quaternion[2] * Quaternion[2] + Quaternion[3] * Quaternion[3]);
              
                /*float yaw = (float)Math.Atan2(2.0f * (Quaternion[1] * Quaternion[2] + Quaternion[0] * Quaternion[3]), 2.0f * (Quaternion[0] * Quaternion[0] + Quaternion[1] * Quaternion[1]) - 1.0f);
                float pitch = -(float)Math.Asin(2.0f * (Quaternion[1] * Quaternion[3] + Quaternion[0] * Quaternion[2]));
                float roll = (float)Math.Atan2(2.0f * (Quaternion[2] * Quaternion[3] - Quaternion[0] * Quaternion[1]), 2.0f * (Quaternion[0] * Quaternion[0] + Quaternion[3] * Quaternion[3]) - 1.0f);
                */
               float q0 = Quaternion[0];
               float q1 = Quaternion[1];
               float q2 = Quaternion[2];
               float q3 = Quaternion[3];

               euler[0] = (float)(Math.Atan2(2 * (q2 * q3) + 2 * q0 * q1, 1 - 2 * q1 * q1 - 2 * q2 * q2) * 180.0f / Math.PI);
               euler[1] = (float)(Math.Asin(2 * q0 * q2 - 2 * q1 * q3) * 180.0f / Math.PI);
               euler[2] = (float)(Math.Atan2(2 * q1 * q2 + 2 * q0 * q3, 1 - 2 * q2 * q2 - 2 * q3 * q3) * 180.0f / Math.PI) + 180.0f + 4.35f; //4.35 degree is the magnetic declination

                //euler[1] = (pitch * 180.0f) / (float)Math.PI;
                //euler[2] = (yaw * 180.0f) / (float)Math.PI; 
                //yaw   -= 13.8; // Declination at Danville, California is 13 degrees 48 minutes and 47 seconds on 2014-04-04
                //euler[0] = (roll * 180.0f) / (float)Math.PI;
                //Console.WriteLine(euler[0] + "," + euler[1] + "," + euler[2]);
                
        }

        float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }
    };
}

