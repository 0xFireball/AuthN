namespace AuthN.Infrastructure.Concurrency {
    public class UserServices {
        public string username { get; }

        /// <summary>
        /// Read/write concurrency lock system
        /// </summary>
        public UserLock userLock { get; }

        public UserServices(string username) {
            this.username = username;
            userLock = new UserLock();
        }
    }
}