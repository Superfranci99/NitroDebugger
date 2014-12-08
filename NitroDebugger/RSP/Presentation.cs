﻿//
//  Presentation.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2014 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace NitroDebugger.RSP
{
	/// <summary>
	/// Represents the Presentation layer.
	/// </summary>
	public class Presentation
	{
		private const int MaxWriteAttemps = 10;
		private const byte PacketSeparator = 0x24;	// '$'

		private Session session;

		public Presentation(string hostname, int port)
		{
			this.session = new Session(hostname, port);
		}

		public void Close()
		{
			this.session.Close();
		}

		public void SendCommand(CommandPacket command)
		{
			this.SendData(PacketBinConverter.ToBinary(command));
		}

		private void SendData(byte[] data)
		{
			int count = 0;
			int response;

			do {
				if (count == MaxWriteAttemps)
					throw new Exception("Can not send packet successfully");
				count++;

				this.session.Write(data);
				response = this.session.ReadByte();
			} while (response != RawPacket.Ack);
		}

		public ReplyPacket SendInterrupt()
		{
			this.session.Write(RawPacket.Interrupt);
			return this.ReceiveReply();
		}

		public ReplyPacket ReceiveReply()
		{
			ReplyPacket response = null;

			while (response != null) {
				try {
					// Get data
					byte[] packet = this.session.ReadPacket(PacketSeparator);
					response = PacketBinConverter.FromBinary(packet);

					// Send ACK
					this.session.Write(RawPacket.Ack);
				} catch (FormatException) {
					// Error... send NACK
					this.session.Write(RawPacket.Nack);
				}
			}

			return response;
		}
	}
}

