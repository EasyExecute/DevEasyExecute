namespace EasyExecute.Tests.TestSystem
{
    public class NameOperationIdObject
    {
        public NameOperationIdObject(string name, uint operationId)
        {
            Name = name;
            Data = operationId;
        }

        public string Name { get; private set; }
        public uint Data { get; private set; }
    }
}