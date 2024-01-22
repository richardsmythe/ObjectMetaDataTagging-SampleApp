namespace ObjectMetaDataTagging.Api.Interfaces
{
    public interface IGenerateTestData
    {
        Task<List<IEnumerable<KeyValuePair<string, object>>>> GenerateTestData();
    }
}
