namespace JevilNet.Services.UserSpecificGuildStorage;

public class UserStorage<TCustomUserStorage>
{
    public ulong UserId { get; set; }
    public string UserName { get; set; }
    public List<TCustomUserStorage> CustomStorage { get; set; } = new();
    
    public UserStorage() {}

    public UserStorage(ulong userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}