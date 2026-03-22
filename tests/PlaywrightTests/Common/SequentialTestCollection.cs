namespace PlaywrightTests.Common;

/// <summary>
/// 測試集合定義，確保測試按順序執行避免並發問題
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialTestCollection
{
    // 這個類別用於定義測試集合，不需要實作任何內容
}
