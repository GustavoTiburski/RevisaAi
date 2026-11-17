using RevisaAi.Model;

namespace RevisaAi.DAO
{
    public class CLembreteDAO : CDAO<CLembrete>
    {
        // Lista todos os registros já ordenados para exibição
        public async Task<List<CLembrete>> ListarOrdenado()
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CLembrete>(
                "SELECT * FROM CLembrete ORDER BY Concluido, DataAlvo");
        }

        public async Task<List<CLembrete>> ListarPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CLembrete>(
                "SELECT * FROM CLembrete WHERE VeiculoId = ? ORDER BY DataAlvo", veiculoId);
        }

        public async Task<int> ExcluirPorVeiculo(int veiculoId)
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteAsync(
                "DELETE FROM CLembrete WHERE VeiculoId = ?", veiculoId);
        }

        // Quantidade de lembretes ainda não concluídos
        public async Task<int> ContarPendentes()
        {
            var db = await CBanco.Conexao();
            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM CLembrete WHERE Concluido = 0");
        }
    }
}
