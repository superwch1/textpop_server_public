namespace MessageWebServer.Hubs
{
    public static class ActiveConnection
    {
        public static List<string> Users = new List<string>();

        /// <summary>
        /// Tuple <userId, otherUserId>
        /// </summary>
        public static Dictionary<string, Tuple<string, string>> Conversations = new Dictionary<string, Tuple<string, string>>();


        public static bool CheckReceiverInConvesation(string senderUserId, string receiverUserId)
        {
            bool receiverInConversation = Conversations.ContainsValue(Tuple.Create(receiverUserId, senderUserId));
            return receiverInConversation;
        }
    }
}
