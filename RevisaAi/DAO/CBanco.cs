using SQLite;
using RevisaAi.Model;

namespace RevisaAi.DAO
{
    // Controla a conexão única com o banco de dados SQLite
    public static class CBanco
    {
        private const string NomeArquivo = "revisaai.db3";

        private static SQLiteAsyncConnection? _db;

        // Retorna a conexão; na primeira chamada cria o arquivo e as tabelas
        public static async Task<SQLiteAsyncConnection> Conexao()
        {
            if (_db != null)
                return _db;

            const SQLiteOpenFlags flags =
                SQLiteOpenFlags.ReadWrite |   // leitura e escrita
                SQLiteOpenFlags.Create |      // cria o arquivo se não existir
                SQLiteOpenFlags.SharedCache;  // acesso compartilhado

            string caminho = Path.Combine(FileSystem.AppDataDirectory, NomeArquivo);

            var db = new SQLiteAsyncConnection(caminho, flags);

            // Cria as 5 tabelas (não faz nada se já existirem)
            await db.CreateTableAsync<CVeiculo>();
            await db.CreateTableAsync<CManutencao>();
            await db.CreateTableAsync<CAbastecimento>();
            await db.CreateTableAsync<CLembrete>();
            await db.CreateTableAsync<COutroGasto>();

            _db = db;
            return _db;
        }
    }
}
