using RevisaAi.Model;

namespace RevisaAi.DAO
{
    public class CVeiculoDAO : CDAO<CVeiculo>
    {
        // Lista os veículos em ordem alfabética
        public async Task<List<CVeiculo>> ListarOrdenado()
        {
            var db = await CBanco.Conexao();
            return await db.QueryAsync<CVeiculo>(
                "SELECT * FROM CVeiculo ORDER BY Marca, Modelo");
        }

        // Procura um veículo pelo Id (null se já foi excluído)
        public async Task<CVeiculo?> BuscarPorId(int id)
        {
            var db = await CBanco.Conexao();
            var lista = await db.QueryAsync<CVeiculo>(
                "SELECT * FROM CVeiculo WHERE Id = ?", id);
            return lista.FirstOrDefault();
        }

        // Procura um veículo pela placa (usado para não deixar repetir placa)
        public async Task<CVeiculo?> BuscarPorPlaca(string placa)
        {
            var db = await CBanco.Conexao();
            var lista = await db.QueryAsync<CVeiculo>(
                "SELECT * FROM CVeiculo WHERE Placa = ?", placa);
            return lista.FirstOrDefault();
        }
    }
}
