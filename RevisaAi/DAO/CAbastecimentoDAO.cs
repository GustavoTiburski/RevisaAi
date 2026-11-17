using RevisaAi.Model;

namespace RevisaAi.DAO
{
    public class CAbastecimentoDAO : CDAO<CAbastecimento>
    {
        // Lista todos os registros já ordenados para exibição
        public async Task<List<CAbastecimento>> ListarOrdenado()
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CAbastecimento>(
                "SELECT * FROM CAbastecimento ORDER BY Data DESC");
        }

        public async Task<List<CAbastecimento>> ListarPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CAbastecimento>(
                "SELECT * FROM CAbastecimento WHERE VeiculoId = ? ORDER BY Data DESC", veiculoId);
        }

        public async Task<int> ExcluirPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteAsync(
                "DELETE FROM CAbastecimento WHERE VeiculoId = ?", veiculoId);
        }

        // Soma de tudo que já foi gasto com combustível
        public async Task<double> TotalGasto()
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteScalarAsync<double>(
                "SELECT IFNULL(SUM(ValorTotal), 0) FROM CAbastecimento");
        }
    }
}
