using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HouseOfTheFuture.RF24Communicator
{
    public sealed class InputStreamReader
    {
        public event EventHandler<string> OnLineRead;

        private DataReader _reader;
        private uint _bufferSize;

        public InputStreamReader(IInputStream stream, uint bufferSize)
        {
            _reader = new DataReader(stream);
            _reader.InputStreamOptions = InputStreamOptions.Partial;

            _bufferSize = bufferSize;
        }

        public async void StartReading()
        {
            while (true)
            {
                await Read();
            }
        }

        private async Task Read()
        {
            try
            {
                var readBytes = await _reader.LoadAsync(_bufferSize).AsTask();
                if (readBytes > 0)
                {
                    var bytes = new byte[readBytes];
                    _reader.ReadBytes(bytes);
                    var message = Encoding.UTF8.GetString(bytes);
                    if (OnLineRead != null)
                        OnLineRead(this, message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
