using System;

namespace Demo.MedTech.DataModel.Exceptions
{
    public class RecordNotFoundException : Exception
    {
        public string Key { get; }

        public string Value { get; }

        public RecordNotFoundException(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}