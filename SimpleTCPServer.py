import asyncio
import random
from enum import IntEnum
import socket

class CommandList(IntEnum):
    StartSampling = 0x01
    StopSampling = 0x02
    StartPrediction = 0x03
    StopPrediction = 0x04
    Init = 0x05
    InitSuccess = 0x06
    Reset = 0x07
    Label = 0x08
    SensorData = 0x09
    FSRData = 0x0A
    LogMessage = 0x0B

class SimpleTCPServer:
    def __init__(self, host='localhost', port=4711):
        self.host = host
        self.port = port
        self.server = None
        self.is_sampling = False
        self.is_predicting = False
        self.predicting_finished = False

    async def start_server(self):
        self.server = await asyncio.start_server(
            self.handle_client, self.host, self.port)

        addr = self.server.sockets[0].getsockname()
        print(f'Serving on {addr}')

        async with self.server:
            await self.server.serve_forever()

    async def handle_client(self, reader, writer):
        while True:
            try:
                data = await asyncio.wait_for(reader.read(100), timeout=0.05)
                command = int.from_bytes(data, byteorder='big')

                if command == CommandList.StartSampling:
                    if(not self.is_predicting):
                        print("Start Sampling received")
                        self.is_sampling = True

                elif command == CommandList.StopSampling:
                    print("Stop Sampling received")
                    self.is_sampling = False
                
                elif command == CommandList.StartPrediction:
                    if(not self.is_sampling):
                        print("Start Prediction received")
                        self.is_predicting = True
                        self.predicting_finished = False

                elif command == CommandList.StopPrediction:
                    if(not self.is_sampling):
                        print("Stop Prediction received")
                        self.predicting_finished = True
                        self.is_predicting = False

            except asyncio.TimeoutError:
                if self.is_sampling:
                    await self.send_random_data(writer)
                if(self.predicting_finished):
                    self.predicting_finished = False
                    await self.send_random_char(writer)

    async def send_random_data(self, writer):
        data = bytearray(8)
        data[0] = CommandList.SensorData
        data[1:] = random.randint(0, 0xFFFFFFFFFFFFFF).to_bytes(7, byteorder='big')
        writer.write(data)
        await writer.drain()

    async def send_random_char(self, writer, ):
        labels_strings = ['A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z']
        data = bytearray(2)
        data[0] = CommandList.Label
        index = random.randint(0, len(labels_strings))
        data[1] = ord(labels_strings[index])
        await asyncio.sleep(1.5)
        print(f"Sending label: {labels_strings[index]}")
        writer.write(data)
        await writer.drain()
        

server = SimpleTCPServer()
asyncio.run(server.start_server())