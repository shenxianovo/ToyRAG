# ToyRAG

武汉大学-2025秋季-自然语言处理-结课作业

## 项目结构

- `ToyRAG.Cli`: 命令行交互界面
- `ToyRAG.Core`: 自制全流程RAG核心类库
  ```
  ToyRAG.Core/
  ├── Entities                    <-- 1. 数据模型
  │   ├── Document.cs                 (原始文档对象)
  │   └── TextChunk.cs                (切分后的片段，包含向量)
  ├── Loading                     <-- 2. 文档加载
  │   ├── IDocumentLoader.cs          (文档加载接口)
  │   └── MarkdownLoader.cs           (Markdown文档加载器)
  ├── Splitting                   <-- 3. 文本切分
  │   ├── ITextSplitter.cs            (文本切分接口)
  │   └── FixedTextSplitter.cs        (固定大小切分器)
  ├── Embeddings                  <-- 4. 向量生成
  │   ├── IEmbeddingGenerator.cs      (向量生成接口)
  │   ├── MiniLMEmbeddingGenerator.cs (MiniLM模型 嵌入生成器)
  │   ├── SimpleBgeTokenizer.cs       (BGE-M3模型 分词器)
  │   └── BGEM3EmbeddingGenerator.cs  (BGE-M3模型 嵌入生成器)
  ├── Storage                     <-- 5. 向量存储
  │   ├── IVectorStore.cs             (向量存储接口)
  │   └── InMemoryVectorStore.cs      (内存存储)
  ├── Generation                  <-- 6. 文本生成
  │   ├── IChatService.cs             (聊天服务接口)
  │   └── GitHubChatService.cs        (调用 GitHub 模型的聊天服务)
  └── Pipelines                   <-- 7. 组装工厂
      └── RAGPipeline.cs              (组装器)
  ```
- `ToyRAG.Core.Tests`: 乱写的杂鱼测试