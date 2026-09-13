# TreinaAi

Sistema de agendamento de horários de treino para uma academia/personal local.

## Sobre o projeto

Atualmente, a academia organiza os horários de treino da semana enviando uma votação manual pelo WhatsApp, um dia antes, para saber quais alunos vão comparecer em cada horário. O TreinaAi nasceu para substituir esse processo manual por um sistema simples, onde os alunos podem visualizar os horários disponíveis de cada professor (segunda a sexta) e se inscrever diretamente, sem depender de mensagens no grupo.

Este é um projeto pessoal de estudo e portfólio, sem fins comerciais — o objetivo foi aprender construindo, do zero, uma solução real para um problema do dia a dia de um comércio local.

## Funcionalidades

- Cadastro e login de usuários com autenticação JWT
- Visualização dos horários disponíveis por professor (segunda a sexta), com controle de vagas em tempo real
- Inscrição em horários de treino, com validações:
- não permite datas passadas
- não permite datas que não correspondam ao dia da semana do horário
- não permite inscrição duplicada no mesmo horário e data
- Tela "Meus agendamentos" para o aluno visualizar e cancelar suas próprias inscrições
- Painel exclusivo para professores, mostrando todos os alunos inscritos em seus horários (incluindo histórico de cancelamentos)
- Apenas professores podem criar novos horários (verificação de permissão no backend)
- Sessão expirada é detectada automaticamente, redirecionando o usuário para o login

## Telas

**Login**

![Tela de login](screenshots/tela-login.png)

**Cadastro**

![Tela de cadastro](screenshots/tela-cadastro.png)

**Horários disponíveis e meus agendamentos**

![Tela de horários](screenshots/tela-agendamentos.png)

**Painel do professor**

![Painel do professor](screenshots/tela-professor.png)

## Tecnologias

**Backend**
- C# / .NET Core
- Entity Framework Core (Code First, com Migrations)
- PostgreSQL
- ASP.NET Core Identity + JWT (autenticação e autorização)
- Swagger (documentação e testes da API)

**Frontend**
- HTML, CSS e JavaScript puro (sem framework)

## Rotas da API

**Auth**
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/Auth/registrar` | Cadastra um novo usuário (sempre como aluno) |
| POST | `/api/Auth/login` | Autentica o usuário e retorna um token JWT |

**HorarioTemplate**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/HorarioTemplate` | Lista os horários disponíveis com vagas calculadas |
| POST | `/api/HorarioTemplate` | Cria um novo horário (somente professores) |
| DELETE | `/api/HorarioTemplate/{id}` | Remove um horário (somente professores) |

**Agendamento**
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/Agendamento` | Cria um agendamento para o usuário autenticado |
| PUT | `/api/Agendamento/{id}/cancelar` | Cancela um agendamento |
| GET | `/api/Agendamento/meus` | Lista os agendamentos do próprio usuário |
| GET | `/api/Agendamento/professor` | Lista os alunos inscritos nos horários do professor logado |

## Como rodar localmente

**Pré-requisitos:** .NET SDK, PostgreSQL

1. Clone o repositório
2. Configure a connection string do PostgreSQL em `TreinaAi.Api/appsettings.json`
3. Dentro de `TreinaAi.Api`, rode as migrations:
   ```bash
   dotnet ef database update
   ```
4. Suba a API:
   ```bash
   dotnet run
   ```
5. Abra `frontend/index.html` no navegador (a API precisa estar rodando em `http://localhost:5133`)

## Status

Projeto funcional e completo em seu escopo inicial (MVP). Próximo passo: deploy em produção usando o GitHub Student Developer Pack.