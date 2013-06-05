using System;
using System.Net.Sockets;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class Server
	{
		public const byte halt		= 0x00;		//end
		public const byte GetPL		= 0x01;		//GetPriseList()
		public const byte GetP		= 0x02;		//GetProduct()
		public const byte GetC		= 0x03;		//GetCash()
		public const byte GetO		= 0x04;		//GetOrder()
		public const byte GetR		= 0x05;		//GetRemainder()
		//public const byte GetPLbT	= 0x06;		//GetProductListByType()
		
		int32 Cash = 0;
		
		void GetData()
		{
			const int NUM_PROD = 16;
			const int BUF_SIZE = 256;
			byte[] buffer = new byte[BUF_SIZE];
			
			private struct Product	{
				int32 code;
				string name;
				decimal price;
				int32 count;
				string type;
			}
			private Product[] ProductList = new Product[NUM_PROD];
			//тут буде заповнення структури з файлу
		}
		
		static void Main( int32 port )
		{
			int32 port = 33333;
			TcpListener listener = new TcpListner( IpAddress.Any, port );
			
			while( true )
			{
				//отримення даних про товари для кожного підключення
				GetData();
				
				listener.Start();
				Console.WriteLine( "Listening on port " + port );
				
				TcpClient client = listener.AcceptTcpClient();
				Console.WriteLine( "Connected from " + ( (IPEndPoint) client.Client.RemoteEndPoint ).Address.ToString()
					+ ":" + ( (IPEndPoint) client.Client.RemoteEndPoint ).Port.ToString() );
					client.GetStream().Read( buffer, 0, BUFF_SIZE );
					
				while( true )
				{
					client.GetStream().Read( buffer, 0, BUFF_SIZE );
					switch( buffer[0] )
					{
						case halt:
							client.Close();
							goto exit;
						case GetPL:
							GetPriceList();
							break;
						case GetP:
							GetProduct( int32 pCode );
							break;
						case GetC:
							GetCash();
							break;
						case GetO:
							GetOrder( int32 pCode );
							break;
						case GetR:
							GetRemainder();
							break;
						case GetPLbT:
							GetProductListByType();
							break;
					}
				}
			exit:
				listener.Stop();
				Console.WriteLine( "Done." );
				Console.Write( "\n" );
				
				Console.WriteLine( "Enter \'c\' for continue, \'s\' to stop server" );
				switch( Console.Read() )
				{
					case 'c':
						break;
					case 's':
						goto stop;
				}
			}
		stop:
			Console.WriteLine( "Server was stopped. Press any key." );
			Console.Read( false );	
		}
		
		static private Product GetPriceList()
		{
			return ProductList;
		}
		static private Product GetProduct( int32 pCode )
		{
			for( int i=0; i < NUM_PROD; i++ )
			{
				if( pCode == ProductList[i].code )
				return ProductList[i];
			}
		}
		static private void GetCash()
		{
			return Cash;
		}
		static private string GetOrder( int32 pCode )
		{
			for( int i=0; i < NUM_PROD; i++ )
			{
				if( pCode == ProductList[i].Product.code )
				return ProductList[i].name;
			}
		}
		static private decimal GetRemainder()
		{
			return Cash;
		}
	}
}
