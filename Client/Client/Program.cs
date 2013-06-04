/*global _comma_separated_list_of_variables_*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Linq;

namespace Client
{
	class Program
	{
		private static IPEndPoint endPoint;

		static void Main( string[] args )
		{
			endPoint = parseNetConfig( args );

		}

		private static IPEndPoint parseNetConfig( string[] args )
		{
			//endPoint = new IPEndPoint();
			JObject config;
			IPAddress ip=null;
			int port=0;

			if( args.Length < 3 ) {
				config = (JObject) JToken.ReadFrom( new JsonTextReader( File.OpenText( "config.json" ) ) );
				if( !int.TryParse( config["netconfig.port"].ToString(), out port ) ) {
					Console.Error.WriteLine( "error: Wrong port in config file." );
					throw new NetConfigNotFoundException();
				}
				if( args.Length < 2 ) {
					if( !IPAddress.TryParse( config["netconfig.port"].ToString(), out ip ) ) {
						Console.Error.WriteLine( "error: Wrong IP in config file." );
						throw new NetConfigNotFoundException();
					}
				} else {
					if( !int.TryParse( args[2], out port ) ) {
						Console.Error.WriteLine( "error: Wrong port." );
						throw new NetConfigNotFoundException();
					}
				}
			} else {
				if( !IPAddress.TryParse( args[1], out ip ) ) {
					Console.Error.WriteLine( "error: Wrong IP." );
					throw new NetConfigNotFoundException();
				}
			}

			return new IPEndPoint( ip, port );
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
