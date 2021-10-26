using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modules
{
    public static class General
    {
        public static OperationResult<T> Deserialize<T>(string SourceData)
        {
            OperationResult<T> op = new OperationResult<T>();
            List<string> errors = new List<string>();

            try
            {
                var t = JsonConvert.DeserializeObject<T>(SourceData,
                    new JsonSerializerSettings
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            errors.Add(args.ErrorContext.Error.Message);
                            args.ErrorContext.Handled = true;
                        },
                        Converters = { new IsoDateTimeConverter() }
                    });

                op.Data = t;
            }
            catch (Exception ex)
            {
                op.Result = false;
                op.Message = ex.Message;
            }
            finally
            {
                if (errors.Count > 0)
                {
                    op.Result = false;
                    op.Message = string.Join(Environment.NewLine, errors.ToArray());
                }
            }

            return op;
        }
    }

    //************************************ OperationResult **********************************
    #region OperationResult
    public class OperationResult
    {
        public bool Result { get; set; }                //Islem basarili / basarisiz...
        public string Message { get; set; }             //Islem basarili / basarisiz...
        //public List<Exception> Exceptions { get; set; } //Islem sirasinda alinan hata (lar) varsa...
        

        public OperationResult()
        {
            this.Result = true;                         //Varsayilan olarak islem her zaman basarili...
            this.Message = "Successful";                //Message NULL dan farkli ataniyor...

            //this.Exceptions = new List<Exception>();    //Exception bos olarak olusturuluyor...
        }
    }
    
    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }
    }
    #endregion OperationResult
}
