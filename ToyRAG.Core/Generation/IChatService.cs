namespace ToyRAG.Core.Generation
{
    public interface IChatService
    {
        /// <summary>
        /// 发送提示词并获取回答
        /// </summary>
        /// <param name="systemPrompt">系统预设 (你是谁，你的任务是...)</param>
        /// <param name="userPrompt">用户问题 + 上下文</param>
        /// <returns>LLM 的回答</returns>
        Task<string> GenerateAsync(string systemPrompt, string userPrompt);
    }
}
