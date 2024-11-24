using MessageWebServer.Database;
using MessageWebServer.Models.Account;
using textpop_server.Models.Account;

namespace MessageWebServer.Repository
{
    public class AccountRepository
    {
        private readonly TextpopDbContext _context;

        public AccountRepository(TextpopDbContext context)
        {
            _context = context;
        }


        public List<UserInfo> ReadUserInfo(string username)
        {
            var users = _context.Users
                .Where(x => x.UserName.Contains(username))
                .Select(x => new UserInfo() { Id = x.Id, Username = x.UserName })
                .ToList();

            return users;
        }

        public List<String> ReadFCMTokenWithUserId(string userId)
        {
            var selectedTokens = _context.FCMTokenInfo
                .Where(x => x.UserId == userId)
                .Select(x => x.FCMToken)
                .ToList();

            return selectedTokens;
        }


        public async Task DeleteFCMTokenWithName(string token)
        {
            var selectedTokens = _context.FCMTokenInfo
                .Where(x => x.FCMToken == token)
                .ToList();

            _context.RemoveRange(selectedTokens);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteFCMTokenWithUserId(string userId)
        {
            var selectedTokens = _context.FCMTokenInfo
                .Where(x => x.UserId == userId)
                .ToList();

            _context.RemoveRange(selectedTokens);
            await _context.SaveChangesAsync();
        }


        public async Task CreateUserFCMToken(string userId, string token)
        {
            var tokensWithSameNameAndUserId = _context.FCMTokenInfo
                .Where(x => x.UserId == userId && x.FCMToken == token)
                .ToList();

            if (tokensWithSameNameAndUserId.Count() > 0)
            {
                return;
            }

            await DeleteFCMTokenWithName(token);

            var tokenInfo = new FCMTokenInfo() { UserId = userId, FCMToken = token, CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() };
            await _context.AddAsync(tokenInfo);
            await _context.SaveChangesAsync();
        }

        
        public async Task CreateBlockedUser(string initiatedUserId, string blockedUserId)
        {
            var blockedInfo = new BlockedInfo() { InitiatedUserId = initiatedUserId, BlockedUserId = blockedUserId };
            await _context.AddAsync(blockedInfo);
            await _context.SaveChangesAsync();
        }


        public bool IsUserBlocked(string user1Id, string user2Id)
        {
            var blockedInfoList = _context.BlockedInfo
                .Where(x => (x.InitiatedUserId == user1Id && x.BlockedUserId == user2Id) || (x.InitiatedUserId == user2Id && x.BlockedUserId == user1Id))
                .ToList();

            if (blockedInfoList.Count > 0)
            {
                return true;
            }
            else
            { 
                return false; 
            }
        }


        public bool IsBlockedUserExist(string initiatedUserId, string blockedUserId)
        {
            var blockedInfoList = _context.BlockedInfo
                .Where(x => x.InitiatedUserId == initiatedUserId && x.BlockedUserId == blockedUserId)
                .ToList();

            if (blockedInfoList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task DeleteBlockedUser(string initiatedUserId, string blockedUserId)
        {
            var blockedInfoList = _context.BlockedInfo
                .Where(x => x.InitiatedUserId == initiatedUserId && x.BlockedUserId == blockedUserId)
                .ToList();

            _context.RemoveRange(blockedInfoList);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAllBlockedInfoWithUserId(string userId)
        {
            var blockedInfoList = _context.BlockedInfo
                .Where(x => x.InitiatedUserId == userId || x.BlockedUserId == userId)
                .ToList();

            _context.RemoveRange(blockedInfoList);
            await _context.SaveChangesAsync();
        }
    }
}
