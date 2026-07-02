# Segfy Insurance - Automobile Policy Web API

Este e um projeto completo desenvolvido em C# (.NET 8) para gerenciar apolices de seguro automovel. O sistema dispoe de uma arquitetura limpa em camadas (Clean/Layered Architecture), validacoes de documentos brasileiros (CPF/CNPJ) e placas veiculares, banco de dados SQLite auto-gerenciado, testes unitarios automatizados com xUnit/Moq, e um front-end moderno integrado diretamente a API.

---

## Tecnologias Utilizadas

1. Backend: .NET 8.0 / C# 12
2. ORM (Persistencia): Entity Framework Core 8.0
3. Banco de Dados: SQLite (Persistido no arquivo local insurance.db)
4. Validacoes: FluentValidation 11.9
5. Testes: xUnit e Moq
6. Frontend: HTML5, CSS3 e Javascript

---

## Arquitetura do Projeto

A solucao foi estruturada de forma modular em projetos:

*   **SegfyInsurance.Domain**: Contem as entidades de dominio (InsurancePolicy), enums (PolicyStatus) e a interface do repositorio (IInsurancePolicyRepository).
*   **SegfyInsurance.Application**: Gerencia as regras de negocio, DTOs, conversores e as validacoes (CPF/CNPJ por algoritmo Modulo 11, placas Mercosul/Legado e consistencia de datas).
*   **SegfyInsurance.Infrastructure**: Implementa a persistencia de dados com EF Core e SQLite, definindo os mapeamentos e executando a consulta SQL nativa.
*   **SegfyInsurance.API**: Controladores RESTful (PoliciesController), configuracoes de injecao de dependencia e hospedagem dos arquivos estaticos do frontend.
*   **SegfyInsurance.Tests**: Conjunto de testes unitarios que garantem a integridade das validacoes e do fluxo de servicos.

---

## Como Executar o Projeto

Gracas ao uso do SQLite e da integracao do frontend aos arquivos estaticos do .NET, nao e necessario instalar ou configurar nada alem do proprio SDK do .NET.

### Pre-requisitos
*   SDK do .NET 8.0 instalado.

### Passo 1: Clonar ou Baixar a Solucao
Abra o terminal no diretorio do projeto.

### Passo 2: Executar a API Web
Execute o comando abaixo para iniciar o servidor web:
```bash
dotnet run --project src/SegfyInsurance.API/SegfyInsurance.API.csproj
```
O console mostrara as URLs em que a API esta escutando, por exemplo:
`Now listening on: http://localhost:5066`

### Passo 3: Acessar a Aplicacao
Abra seu navegador e acesse o endereco fornecido (ex: http://localhost:5066). O front-end sera carregado automaticamente e voce podera:
*   Visualizar a lista de apolices cadastradas.
*   Criar novas apolices com preenchimento assistido por mascara automatica de CPF/CNPJ e placa.
*   Editar os dados de uma apolice.
*   Cancelar a vigencia (altera o status para "Cancelada").
*   Excluir apolices.
*   Alternar a aba para filtrar apolices que vencem nos primeiros 30 dias (usando a consulta SQL).

---

## Como Executar os Testes Unitarios

Para rodar o conjunto de 15 testes unitarios e verificar as regras de validacao:
```bash
dotnet test
```

---

## Consulta SQL Requerida (Vencimento nos proximos 30 dias)

Conforme solicitado pela atividade, a listagem de apolices que vencem nos proximos 30 dias executa uma consulta SQL nativa.
Ela e executada no banco SQLite atraves do metodo GetExpiringIn30DaysAsync() da classe InsurancePolicyRepository utilizando FromSqlRaw:

```sql
SELECT * FROM Policies 
WHERE Status = 0 
  AND Date(DataFimVigencia) >= Date('now') 
  AND Date(DataFimVigencia) <= Date('now', '+30 days')
```

*(Onde Status = 0 representa apolices com status Ativa).*

---

## Destaques de Engenharia e Design
*   **Auto-inicializacao**: O banco de dados SQLite (insurance.db) e suas tabelas sao criados automaticamente na primeira inicializacao da API, sem necessidade de rodar comandos de migration adicionais.
*   **Thread-Safety na Sequencia**: A geracao automatica do numero da apolice (SEG-YYYY-XXXX) le e incrementa sequencialmente as apolices do ano corrente no banco de dados.
*   **Design Neutro**: O front-end foi construido em CSS moderno utilizando paineis solidos cinza-escuro com bordas finas e cores neutras, priorizando a sobriedade e a legibilidade.
