using RevisaAi.Model;

namespace RevisaAi.DAO
{
    public class COutroGastoDAO : CDAO<COutroGasto>
    {
        // Lista todos os registros já ordenados para exibição
        public async Task<List<COutroGasto>> ListarOrdenado()
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<COutroGasto>(
                "SELECT * FROM COutroGasto ORDER BY Data DESC");
        }

        public async Task<List<COutroGasto>> ListarPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<COutroGasto>(
                "SELECT * FROM COutroGasto WHERE VeiculoId = ? ORDER BY Data DESC", veiculoId);
        }

        public async Task<int> ExcluirPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteAsync(
                "DELETE FROM COutroGasto WHERE VeiculoId = ?", veiculoId);
        }

        // Soma dos outros gastos que já foram pagos
        public async Task<double> TotalGasto()
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteScalarAsync<double>(
                "SELECT IFNULL(SUM(Valor), 0) FROM COutroGasto WHERE Pago = 1");
        }
    }
}
