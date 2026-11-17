namespace RevisaAi.DAO
{
    // DAO genérico com o CRUD básico, reaproveitado pelas 4 tabelas
    public class CDAO<T> where T : new()
    {
        // CREATE
        public async Task<int> Inserir(T item)
        {
            var db = await CBanco.Conexao();
            return await db.InsertAsync(item);
        }

        // UPDATE
        public async Task<int> Alterar(T item)
        {
            var db = await CBanco.Conexao();
            return await db.UpdateAsync(item);
        }

        // DELETE
        public async Task<int> Excluir(T item)
        {
            var db = await CBanco.Conexao();
            return await db.DeleteAsync(item);
        }

        // READ (todos os registros)
        public async Task<List<T>> Listar()
        {
            var db = await CBanco.Conexao();
            return await db.Table<T>().ToListAsync();
        }

        // Quantidade de registros da tabela
        public async Task<int> Contar()
        {
            var db = await CBanco.Conexao();
            return await db.Table<T>().CountAsync();
        }
    }
}
