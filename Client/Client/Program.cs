/*global _comma_separated_list_of_variables_*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
	class Program
	{
		private static IPEndPoint endPoint;
		private static TcpClient client;
		private static NetworkStream clientStream;
		const int BUF_SIZE = 256;

		static void Main( string[] args )
		{
			endPoint = parseNetConfig( args );

			client = new TcpClient();

			try {
				client.Connect( endPoint );
			}
			catch( SocketException ) {
				Console.Error.WriteLine( "error: connection problem." );
				Environment.Exit( 1 );
			}

			clientStream = client.GetStream();
			client.SendBufferSize = BUF_SIZE;

			Console.WriteLine( "Connected to " + endPoint.Address.ToString()
					+ ":" + endPoint.Port.ToString() );

			Console.WriteLine( "Enter commands (type \"help\" for help ):" );

			while( true ) {
				Console.Write( ">>> " );
				string[] command = new string[1];
				command[0] = Console.ReadLine();
				command[0].Trim();

				command = command[0].Split( ' ', '\t' );
				switch( command[0].ToLower() ) {
					case ( "help" ):
						showHelp();
						break;
					case ( "ls" ):
						showProductList( command );
						break;
					case ( "print" ):
						if( command[1].Equals( "cash", StringComparison.CurrentCultureIgnoreCase ) ) {
							showCash();
						} else {
							Console.Error.WriteLine( "error: Wrong parameter." );
						}
						break;
					case ( "buy" ):
						doBuy( command );
						break;
					case ( "exit" ):
					case ( "quit" ):
						goto EXIT;
					default:
						Console.Error.WriteLine( "error: Wrong command." );
						break;
				}
			}

		EXIT:
			clientStream.Write( Enumerable.Repeat( (byte) Command.Halt, BUF_SIZE ).ToArray(), 0, BUF_SIZE );
			clientStream.Close( 500 );
			client.Close();
		}

		private static void doBuy( string[] command )
		{
			byte[] buffer = new byte[BUF_SIZE];
			byte[] type;
			int code;

			if( !int.TryParse( command[1], out code ) ) {
				type = GetBytes( command[0] );
				buffer[0] = (byte) Command.GetProductListByType;
				buffer[1] = (byte) type.Length;
				type.CopyTo( buffer, 2 );
				clientStream.Read( buffer, 0, sizeof( int ) );
				Array.Resize( ref buffer, BitConverter.ToInt32( buffer, 0 ) );

				clientStream.Read( buffer, 0, buffer.Length );

				Product prod = new Product();
				for( int i = 0; i < buffer.Length; i++ ) {
					if( buffer[i] == 0 ) {
						break;
					}
					prod.code = BitConverter.ToInt32( buffer, i );
					i += sizeof( int );
					prod.name = GetString( buffer,
						BitConverter.ToInt32( buffer, i ),
						i + sizeof( int ) );
					i += sizeof( int ) + prod.name.Length;
					prod.price = new BinaryReader( new MemoryStream( buffer, i, sizeof( decimal ) ) ).ReadDecimal();
					i += sizeof( decimal );
					prod.quantity = BitConverter.ToInt32( buffer, i );
					i += sizeof( int );
					prod.type = GetString( buffer,
						BitConverter.ToInt32( buffer, i ),
						i + sizeof( int ) );
					i += sizeof( int ) + prod.name.Length;
					//Console.WriteLine( "Code: " + prod.code + " Name: " + prod.name
					//	+ " Price: " + prod.price + " Quantity: " + prod.quantity + " Type: " + prod.type );
					if( prod.name.Equals( command[0] ) ) {
						code = prod.code;
					}
				}

			}

			type = BitConverter.GetBytes( code );
			buffer[0] = (byte) Command.GetOrder;
			type.CopyTo( buffer, 1 );
			clientStream.Write( buffer, 0, buffer.Length );
		}

		private static void showCash()
		{
			byte[] buffer = new byte[BUF_SIZE];

			buffer[0] = (byte) Command.GetCash;
			clientStream.Write( buffer, 0, buffer.Length );
			clientStream.Read( buffer, 0, buffer.Length );
			Console.WriteLine( "Cash: " + BitConverter.ToInt32( buffer, 0 ) );
		}

		private static void showProductList( string[] command )
		{
			byte[] buffer = new byte[BUF_SIZE];
			int code;
			byte[] type;
			Command queryType;

			if( command.Length > 1 ) {
				if( int.TryParse( command[1], out code ) ) {
					queryType = Command.GetProduct;
					type = BitConverter.GetBytes( code );
					buffer[0] = (byte) Command.GetProduct;
					type.CopyTo( buffer, 1 );
				} else {
					queryType = Command.GetProductListByType;
					type = GetBytes( command[0] );
					buffer[0] = (byte) Command.GetProductListByType;
					buffer[1] = (byte) type.Length;
					type.CopyTo( buffer, 2 );
				}
			} else {
				queryType = Command.GetProductList;
				buffer[0] = (byte) Command.GetProductList;
			}

			clientStream.Write( buffer, 0, buffer.Length );

			if( queryType != Command.GetProduct ) {
				clientStream.Read( buffer, 0, sizeof( int ) );
				Array.Resize( ref buffer, BitConverter.ToInt32( buffer, 0 ) );
			}
			clientStream.Read( buffer, 0, buffer.Length );

			Product prod = new Product();
			for( int i = 0; i < buffer.Length; i++ ) {
				if( buffer[i] == 0 ) {
					break;
				}
				prod.code = BitConverter.ToInt32( buffer, i );
				i += sizeof( int );
				prod.name = GetString( buffer,
					BitConverter.ToInt32( buffer, i ),
					i + sizeof( int ) );
				i += sizeof( int ) + prod.name.Length;
				prod.price = new BinaryReader( new MemoryStream( buffer, i, sizeof( decimal ) ) ).ReadDecimal();
				i += sizeof( decimal );
				prod.quantity = BitConverter.ToInt32( buffer, i );
				i += sizeof( int );
				prod.type = GetString( buffer,
					BitConverter.ToInt32( buffer, i ),
					i + sizeof( int ) );
				i += sizeof( int ) + prod.name.Length;
				Console.WriteLine( "Code: " + prod.code + " Name: " + prod.name
					+ " Price: " + prod.price + " Quantity: " + prod.quantity + " Type: " + prod.type );
			}

		}

		private static void showHelp()
		{
			Console.WriteLine( "Usage:\n" +
				"ls [(type|code)]\n" +
				"buy (code|name)\n"
				);
		}

		private static IPEndPoint parseNetConfig( string[] args )
		{
			JObject config = null;
			IPAddress ip = null;
			int port = 0;

			if( args.Length < 2 ) {
				try {
					config = (JObject) JToken.ReadFrom( new JsonTextReader( File.OpenText( @"config.json" ) ) );
				}
				catch( FileNotFoundException ) {
					Console.Error.WriteLine( "error: No \"config.json\" file." );
					throw new FileNotFoundException();
				}


				if( !int.TryParse( config.SelectToken( "netconfig.port" ).ToString(), out port ) ) {
					Console.Error.WriteLine( "error: Wrong port in config file." );
					throw new NetConfigNotFoundException();
				}
				if( args.Length < 1 ) {
					if( !IPAddress.TryParse( config.SelectToken( "netconfig.ip" ).ToString(), out ip ) ) {
						Console.Error.WriteLine( "error: Wrong IP in config file." );
						throw new NetConfigNotFoundException();
					}
				} else {
					if( !int.TryParse( args[1], out port ) ) {
						Console.Error.WriteLine( "error: Wrong port." );
						throw new NetConfigNotFoundException();
					}
				}
			} else {
				if( !IPAddress.TryParse( args[0], out ip ) ) {
					Console.Error.WriteLine( "error: Wrong IP." );
					throw new NetConfigNotFoundException();
				}
				if( !int.TryParse( args[1], out port ) ) {
					Console.Error.WriteLine( "error: Wrong port." );
					throw new NetConfigNotFoundException();
				}
			}

			return new IPEndPoint( ip, port );
		}

		internal enum Command
		{
			Halt = 0x00,		//end
			GetProductList = 0x01,		//GetProductList()
			GetProduct = 0x02,		//GetProduct()
			GetCash = 0x03,		//GetCash()
			GetOrder = 0x04,		//GetOrder()
			GetRemainder = 0x05,		//GetRemainder()
			GetProductListByType = 0x06		//GetProductListByType()
		}

		public struct Product
		{
			public int code;
			public string name;
			public decimal price;
			public int quantity;
			public string type;
		}

		static byte[] GetBytes( string str )
		{
			byte[] bytes = new byte[str.Length * sizeof( char )];
			System.Buffer.BlockCopy( str.ToCharArray(), 0, bytes, 0, bytes.Length );
			return bytes;
		}

		static string GetString( byte[] bytes, int size = 0, int start = 0 )
		{
			if( size == 0 ) {
				size = bytes.Length / sizeof( char );
			}
			char[] chars = new char[size];
			System.Buffer.BlockCopy( bytes, start, chars, 0, size );
			return new string( chars );
		}
	}

	class NetConfigNotFoundException : Exception
	{
		public NetConfigNotFoundException()
		{
		}

		public NetConfigNotFoundException( string message )
			: base( message )
		{
		}

		public NetConfigNotFoundException( string message, Exception inner )
			: base( message, inner )
		{
		}
	}
}
