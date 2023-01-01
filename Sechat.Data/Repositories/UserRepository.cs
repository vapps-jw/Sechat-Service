namespace Sechat.Data.Repositories
{
    public class UserRepository : RepositoryBase<SechatContext>
    {
        public UserRepository(SechatContext context) : base(context)
        {
        }
    }
}
