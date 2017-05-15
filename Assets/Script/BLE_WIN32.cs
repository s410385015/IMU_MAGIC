using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class BLE_WIN32 : MonoBehaviour {

	public class Win32Com
	{
		private IntPtr _hPort;
		const Int32 DATA_BUFFER = 409600000;
		const UInt32 MAXDWORD = 0xffffffff;
		const UInt32 PURGE_TXCLEAR = 0x4;
		const UInt32 PURGE_RXCLEAR = 0x8;
		/// <summary>
		/// Opening Testing and Closing the Port Handle.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		//Constants for errors:
		const UInt32 ERROR_FILE_NOT_FOUND = 2;
		const UInt32 ERROR_INVALID_NAME = 123;
		const UInt32 ERROR_ACCESS_DENIED = 5;
		const UInt32 ERROR_IO_PENDING = 997;

		//Constants for return value:
		const Int32 INVALID_HANDLE_VALUE = -1;

		//Constants for dwFlagsAndAttributes:
		const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;

		//Constants for dwCreationDisposition:
		const UInt32 OPEN_EXISTING = 3;

		//Constants for dwDesiredAccess:
		const UInt32 GENERIC_READ = 0x80000000;
		const UInt32 GENERIC_WRITE = 0x40000000;

		[DllImport("kernel32.dll")]
		static extern Boolean CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		static extern Boolean GetHandleInformation(IntPtr hObject, out UInt32 lpdwFlags);


		/// <summary>
		/// Manipulating the communications settings.
		/// </summary>

		[DllImport("kernel32.dll")]
		static extern Boolean GetCommState(IntPtr hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		static extern Boolean GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		static extern Boolean BuildCommDCBAndTimeouts(String lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		static extern Boolean SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		static extern Boolean SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		static extern Boolean SetupComm(IntPtr hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

		[DllImport("kernel32.dll")]
		static extern Boolean PurgeComm(IntPtr hFile, UInt32 dwFlags);
		/// <summary>
		/// Reading and writing.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern Boolean WriteFile(IntPtr fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
			out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		static extern Boolean SetCommMask(IntPtr hFile, UInt32 dwEvtMask);

		// Constants for dwEvtMask:
		const UInt32 EV_RXCHAR = 0x0001;
		const UInt32 EV_RXFLAG = 0x0002;
		const UInt32 EV_TXEMPTY = 0x0004;
		const UInt32 EV_CTS = 0x0008;
		const UInt32 EV_DSR = 0x0010;
		const UInt32 EV_RLSD = 0x0020;
		const UInt32 EV_BREAK = 0x0040;
		const UInt32 EV_ERR = 0x0080;
		const UInt32 EV_RING = 0x0100;
		const UInt32 EV_PERR = 0x0200;
		const UInt32 EV_RX80FULL = 0x0400;
		const UInt32 EV_EVENT1 = 0x0800;
		const UInt32 EV_EVENT2 = 0x1000;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern Boolean WaitCommEvent(IntPtr hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		static extern Boolean CancelIo(IntPtr hFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern Boolean ReadFile(IntPtr hFile, [Out] Byte[] lpBuffer, UInt32 nNumberOfBytesToRead,
			out UInt32 nNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		static extern Boolean TransmitCommChar(IntPtr hFile, Byte cChar);

		/// <summary>
		/// Control port functions.
		/// </summary>
		[DllImport("kernel32.dll")]
		static extern Boolean EscapeCommFunction(IntPtr hFile, UInt32 dwFunc);

		// Constants for dwFunc:
		const UInt32 SETXOFF = 1;
		const UInt32 SETXON = 2;
		const UInt32 SETRTS = 3;
		const UInt32 CLRRTS = 4;
		const UInt32 SETDTR = 5;
		const UInt32 CLRDTR = 6;
		const UInt32 RESETDEV = 7;
		const UInt32 SETBREAK = 8;
		const UInt32 CLRBREAK = 9;

		[DllImport("kernel32.dll")]
		static extern Boolean GetCommModemStatus(IntPtr hFile, out UInt32 lpModemStat);

		// Constants for lpModemStat:
		const UInt32 MS_CTS_ON = 0x0010;
		const UInt32 MS_DSR_ON = 0x0020;
		const UInt32 MS_RING_ON = 0x0040;
		const UInt32 MS_RLSD_ON = 0x0080;

		/// <summary>
		/// Status Functions.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern Boolean GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped,
			out UInt32 nNumberOfBytesTransferred, Boolean bWait);

		[DllImport("kernel32.dll")]
		static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, IntPtr lpStat);
		[DllImport("kernel32.dll")]
		static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, out COMSTAT cs);

		//Constants for lpErrors:
		const UInt32 CE_RXOVER = 0x0001;
		const UInt32 CE_OVERRUN = 0x0002;
		const UInt32 CE_RXPARITY = 0x0004;
		const UInt32 CE_FRAME = 0x0008;
		const UInt32 CE_BREAK = 0x0010;
		const UInt32 CE_TXFULL = 0x0100;
		const UInt32 CE_PTO = 0x0200;
		const UInt32 CE_IOE = 0x0400;
		const UInt32 CE_DNS = 0x0800;
		const UInt32 CE_OOP = 0x1000;
		const UInt32 CE_MODE = 0x8000;
		[DllImport("kernel32.dll")]
		static extern Boolean GetCommProperties(IntPtr hFile, out COMMPROP cp);
		[StructLayout(LayoutKind.Sequential)]
		struct OVERLAPPED
		{
			UIntPtr Internal;
			UIntPtr InternalHigh;
			UInt32 Offset;
			UInt32 OffsetHigh;
			IntPtr hEvent;
		}
		[StructLayout(LayoutKind.Sequential)]
		internal struct DCB
		{
			internal UInt32 DCBlength;      /* sizeof(DCB)                     */
			internal Int32 BaudRate;       /* Baudrate at which running       */
			internal UInt32 fBinary;     /* Binary Mode (skip EOF check)    */
			internal UInt32 fParity;     /* Enable parity checking          */
			internal UInt32 fOutxCtsFlow; /* CTS handshaking on output       */
			internal UInt32 fOutxDsrFlow; /* DSR handshaking on output       */
			internal UInt32 fDtrControl;  /* DTR Flow control                */
			internal UInt32 fDsrSensitivity; /* DSR Sensitivity              */
			internal UInt32 fTXContinueOnXoff; /* Continue TX when Xoff sent */
			internal UInt32 fOutX;       /* Enable output X-ON/X-OFF        */
			internal UInt32 fInX;        /* Enable input X-ON/X-OFF         */
			internal UInt32 fErrorChar;  /* Enable Err Replacement          */
			internal UInt32 fNull;       /* Enable Null stripping           */
			internal UInt32 fRtsControl;  /* Rts Flow control                */
			internal UInt32 fAbortOnError; /* Abort all reads and writes on Error */
			internal UInt32 fDummy2;     /* Reserved                        */
			internal UInt16 wReserved;       /* Not currently used              */
			internal UInt16 XonLim;          /* Transmit X-ON threshold         */
			internal UInt16 XoffLim;         /* Transmit X-OFF threshold        */
			internal Byte ByteSize;        /* Number of bits/byte, 4-8        */
			internal Byte Parity;          /* 0-4=None,Odd,Even,Mark,Space    */
			internal Byte StopBits;        /* 0,1,2 = 1, 1.5, 2               */
			internal Byte XonChar;         /* Tx and Rx X-ON character        */
			internal Byte XoffChar;        /* Tx and Rx X-OFF character       */
			internal Byte ErrorChar;       /* Error replacement char          */
			internal Byte EofChar;         /* End of Input character          */
			internal Byte EvtChar;         /* Received Event character        */
			internal Int16 wReserved1;      /* Fill for now.                  */
		}
		[StructLayout(LayoutKind.Sequential)]
		struct COMSTAT
		{
			const uint fCtsHold = 0x1;
			const uint fDsrHold = 0x2;
			const uint fRlsdHold = 0x4;
			const uint fXoffHold = 0x8;
			const uint fXoffSent = 0x10;
			const uint fEof = 0x20;
			const uint fTxim = 0x40;
			UInt32 Flags;
			UInt32 cbInQue;
			UInt32 cbOutQue;
		}
		[StructLayout(LayoutKind.Sequential)]
		internal struct COMMTIMEOUTS
		{
			internal UInt32 ReadIntervalTimeout;
			internal UInt32 ReadTotalTimeoutMultiplier;
			internal UInt32 ReadTotalTimeoutConstant;
			internal UInt32 WriteTotalTimeoutMultiplier;
			internal UInt32 WriteTotalTimeoutConstant;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct COMMPROP
		{
			UInt16 wPacketLength;
			UInt16 wPacketVersion;
			UInt32 dwServiceMask;
			UInt32 dwReserved1;
			UInt32 dwMaxTxQueue;
			UInt32 dwMaxRxQueue;
			UInt32 dwMaxBaud;
			UInt32 dwProvSubType;
			UInt32 dwProvCapabilities;
			UInt32 dwSettableParams;
			UInt32 dwSettableBaud;
			UInt16 wSettableData;
			UInt16 wSettableStopParity;
			UInt32 dwCurrentTxQueue;
			UInt32 dwCurrentRxQueue;
			UInt32 dwProvSpec1;
			UInt32 dwProvSpec2;
			Byte wcProvChar;
		}
		public bool OpenCom(String strPort, int iBaudRate)
		{
			var portDcb = new DCB();
			var commTimeouts = new COMMTIMEOUTS();
			_hPort = Win32Com.CreateFile(strPort,
				Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 
				0, 
				IntPtr.Zero,
				Win32Com.OPEN_EXISTING,
				0, 
				IntPtr.Zero);
			if (_hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
			{
				return false;
			}
			if (GetCommState(_hPort, ref portDcb))
			{
				portDcb.BaudRate = iBaudRate;
				portDcb.ByteSize = (byte)8;
				portDcb.Parity = (byte)0;
				portDcb.StopBits = (byte)0;
				portDcb.fNull = 0;
				portDcb.fParity = 0;/*
                portDcb.XonLim = 32768;
                portDcb.XoffLim = 8192;
                portDcb.XonChar= 17;
                portDcb.XoffChar = 19;
                portDcb.XoffChar = 19;
                portDcb.fOutxCtsFlow = 0;
                portDcb.fOutxDsrFlow  = 0;*/
			}
			else
			{
				CloseHandle(_hPort);
				return false;
			}
			SetupComm(_hPort, DATA_BUFFER, DATA_BUFFER);
			if (!SetCommState(_hPort, ref portDcb))
			{
				CloseHandle(_hPort);
				return false;
			}
			GetCommTimeouts(_hPort, out commTimeouts);

			commTimeouts.ReadIntervalTimeout = MAXDWORD;
			commTimeouts.ReadTotalTimeoutConstant = 0;
			commTimeouts.ReadTotalTimeoutMultiplier = 0;
			commTimeouts.WriteTotalTimeoutConstant = MAXDWORD;
			commTimeouts.WriteTotalTimeoutMultiplier = 0;

			// Set the time-out parameters for all read and write operations on the port. 
			if (!SetCommTimeouts(_hPort, ref commTimeouts))
			{
				// Could not create the read thread. 
				CloseHandle(_hPort);
				return false;
			} 

			// Clear the port of any existing data. 
			PurgeComm(_hPort, PURGE_TXCLEAR | PURGE_RXCLEAR);

			return true;
		}

		public bool WriteCom(byte[] toSend,uint uiCount)
		{
			uint sent;

			Win32Com.WriteFile(_hPort,	        // Port handle 
				toSend,			// Pointer to the data to write 
				uiCount,		// Number of bytes to write 
				out sent,	    // Pointer to the number of bytes written 
				IntPtr.Zero);	// Must be NULL 

			if (sent > 0)
			{
				return true;					//Transmission was success
			}
			else
			{
				return false;
			}
		}
		public Int32 ReadCom(byte[] toSend,int uiCount)
		{
			bool bRes = false, bComEvent;
			UInt32 retlen;
			bComEvent = Win32Com.SetCommMask (_hPort, EV_RXCHAR);
			if (bComEvent)
			{
				bRes = ReadFile(_hPort,		// handle of file to read
					toSend,	    // pointer to buffer that receives data
					(uint)uiCount,		// number of bytes to read
					out retlen,	// pointer to number of bytes read
					IntPtr.Zero	// pointer to structure for data
				);
				if (!(bRes))
				{
					return 0;
				}


				if (retlen > 0)				//If we have data
				{
					return (Int32)retlen;	//return the length
				}
				else
				{
					return 0;				//else no data has been read
				}
			}
			else
			{
				return 0;
			}
		}

		public void CloseCom()
		{
			CloseHandle(_hPort);
			_hPort = IntPtr.Zero;
		}
	}
}
