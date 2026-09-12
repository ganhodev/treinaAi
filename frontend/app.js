const API_URL = "http://localhost:5133/api";

function mostrarCadastro() {
    document.getElementById("form-login").style.display = "none";
    document.getElementById("form-cadastro").style.display = "block";
}

function mostrarLogin() {
    document.getElementById("form-cadastro").style.display = "none";
    document.getElementById("form-login").style.display = "block";
}

async function fazerLogin() {
    const email = document.getElementById("login-email").value;
    const senha = document.getElementById("login-senha").value;

    try {
        const resposta = await fetch(`${API_URL}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, senha })
        });

        if (!resposta.ok) {
            alert("Email ou senha inválidos.");
            return;
        }

        const dados = await resposta.json();

        localStorage.setItem("token", dados.token);
        localStorage.setItem("nome", dados.nome);
        localStorage.setItem("ehProfessor", dados.ehProfessor);

        window.location.href = "horarios.html";
    } catch (erro) {
        alert("Erro ao conectar com o servidor.");
        console.error(erro);
    }
}

async function fazerCadastro() {
    const nome = document.getElementById("cadastro-nome").value;
    const email = document.getElementById("cadastro-email").value;
    const senha = document.getElementById("cadastro-senha").value;

    try {
        const resposta = await fetch(`${API_URL}/Auth/registrar`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ nome, email, senha, ehProfessor: false })
        });

        if (!resposta.ok) {
            const erro = await resposta.json();
            alert("Erro ao cadastrar: " + JSON.stringify(erro.errors || erro));
            return;
        }

        alert("Cadastro realizado! Agora faça login.");
        mostrarLogin();
    } catch (erro) {
        alert("Erro ao conectar com o servidor.");
        console.error(erro);
    }
}

function verificarLogin() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "index.html";
        return null;
    }
    return token;
}

function sair() {
    localStorage.clear();
    window.location.href = "index.html";
}

async function carregarHorarios() {
    const token = verificarLogin();
    if (!token) return;

    document.getElementById("nome-usuario").textContent = localStorage.getItem("nome");

    const ehProfessor = localStorage.getItem("ehProfessor") === "true";
    if (ehProfessor) {
        document.getElementById("link-professor").style.display = "block";
    }

    try {
        const resposta = await fetch(`${API_URL}/HorarioTemplate`);
        const horarios = await resposta.json();

        const container = document.getElementById("lista-horarios");
        container.innerHTML = "";

        horarios.forEach(h => {
            const semVaga = h.vagasDisponiveis <= 0;

            const card = document.createElement("div");
            card.className = "horario-card";
            card.innerHTML = `
                <div class="horario-info">
                    <strong>${h.diaSemana}</strong>
                    ${h.horaInicio} - ${h.horaFim} · Prof. ${h.professorNome}
                    <br>Vagas: ${h.vagasDisponiveis}/${h.capacidadeMaxima}
                </div>
                <button class="btn-inscrever ${semVaga ? 'sem-vaga' : ''}"
                        ${semVaga ? 'disabled' : ''}
                        onclick="inscrever(${h.id}, '${h.diaSemana}')">
                    ${semVaga ? 'Sem vaga' : 'Inscrever-se'}
                </button>
            `;
            container.appendChild(card);
        });
    } catch (erro) {
        console.error(erro);
        document.getElementById("lista-horarios").innerHTML = "<p>Erro ao carregar horários.</p>";
    }
}

const diasSemanaJs = {
    "segunda": 1,
    "terca": 2,
    "quarta": 3,
    "quinta": 4,
    "sexta": 5
};

let horarioSelecionado = null;
let diaSemanaSelecionado = null;

function proximasDatas(diaSemana) {
    const diaAlvo = diasSemanaJs[diaSemana];
    const datas = [];
    const hoje = new Date();
    const limite = new Date(hoje.getFullYear(), 11, 31);

    let data = new Date(hoje);
    while (data <= limite) {
        data.setDate(data.getDate() + 1);
        if (data.getDay() === diaAlvo && data <= limite) {
            const ano = data.getFullYear();
            const mes = String(data.getMonth() + 1).padStart(2, "0");
            const dia = String(data.getDate()).padStart(2, "0");
            datas.push(`${ano}-${mes}-${dia}`);
        }
    }
    return datas;
}

function inscrever(horarioTemplateId, diaSemana) {
    horarioSelecionado = horarioTemplateId;
    diaSemanaSelecionado = diaSemana;

    const hoje = new Date();
    const limite = new Date(hoje.getFullYear(), 11, 31);

    const input = document.getElementById("input-data");
    input.min = hoje.toISOString().split("T")[0];
    input.max = limite.toISOString().split("T")[0];
    input.value = proximasDatas(diaSemana)[0];

    document.getElementById("modal-agendar").style.display = "flex";
}

function fecharModal() {
    document.getElementById("modal-agendar").style.display = "none";
    horarioSelecionado = null;
    diaSemanaSelecionado = null;
}

async function confirmarAgendamento() {
    const token = verificarLogin();
    if (!token) return;

    const data = document.getElementById("input-data").value;

    const dataEscolhida = new Date(data + "T00:00:00");
    if (dataEscolhida.getDay() !== diasSemanaJs[diaSemanaSelecionado]) {
        alert(`Escolha uma data que caia em uma ${diaSemanaSelecionado}-feira.`);
        return;
    }

    try {
        const resposta = await fetch(`${API_URL}/Agendamento`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ horarioTemplateId: horarioSelecionado, data })
        });

        if (!resposta.ok) {
            const erro = await resposta.text();
            alert("Erro ao inscrever: " + erro);
            return;
        }

        alert("Inscrito com sucesso!");
        fecharModal();
        carregarHorarios();
    } catch (erro) {
        alert("Erro ao conectar com o servidor.");
        console.error(erro);
    }
}

async function carregarAgendamentosProfessor() {
    const token = verificarLogin();
    if (!token) return;

    const ehProfessor = localStorage.getItem("ehProfessor") === "true";
    if (!ehProfessor) {
        alert("Acesso restrito a professores.");
        window.location.href = "horarios.html";
        return;
    }

    try {
        const resposta = await fetch(`${API_URL}/Agendamento/professor`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!resposta.ok) {
            document.getElementById("lista-agendamentos").innerHTML = "<p>Erro ao carregar dados.</p>";
            return;
        }

        const agendamentos = await resposta.json();
        const container = document.getElementById("lista-agendamentos");

        if (agendamentos.length === 0) {
            container.innerHTML = "<p>Nenhum agendamento ainda.</p>";
            return;
        }

        container.innerHTML = "";
        agendamentos.forEach(a => {
            const statusClass = a.status === "Confirmado" ? "status-confirmado" : "status-cancelado";
            const card = document.createElement("div");
            card.className = "agendamento-card";
            card.innerHTML = `
                <strong>${a.alunoNome}</strong><br>
                Data: ${a.data} · ${a.horaInicio} - ${a.horaFim}<br>
                Status: <span class="${statusClass}">${a.status}</span>
            `;
            container.appendChild(card);
        });
    } catch (erro) {
        console.error(erro);
        document.getElementById("lista-agendamentos").innerHTML = "<p>Erro ao conectar com o servidor.</p>";
    }
}