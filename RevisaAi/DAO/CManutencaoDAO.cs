using RevisaAi.Model;

namespace RevisaAi.DAO
{
    public class CManutencaoDAO : CDAO<CManutencao>
    {
        // Lista todos os registros já ordenados para exibição
        public async Task<List<CManutencao>> ListarOrdenado()
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CManutencao>(
                "SELECT * FROM CManutencao ORDER BY Data DESC");
        }

        public async Task<List<CManutencao>> ListarPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CManutencao>(
                "SELECT * FROM CManutencao WHERE VeiculoId = ? ORDER BY Data DESC", veiculoId);
        }

        public async Task<int> ExcluirPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteAsync(
                "DELETE FROM CManutencao WHERE VeiculoId = ?", veiculoId);
        }

        // Soma de tudo que já foi gasto com manutenção
        public async Task<double> TotalGasto()
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteScalarAsync<double>(
                "SELECT IFNULL(SUM(Valor), 0) FROM CManutencao");
        }
    }
}
