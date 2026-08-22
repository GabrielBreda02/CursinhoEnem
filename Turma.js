// ===============================
// Script da Página de Gerenciamento de uma Turma (professor)
// ===============================

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const nomeTurmaInput = document.getElementById("nomeTurma");
const selecionarAlunoSelect = document.getElementById("selecionarAluno");
const listaAlunos = document.getElementById("listaAlunos");

if (!id) {
    alert("Turma não encontrada.");
} else {
    carregarTurma();
    carregarAlunosDisponiveis();
}

function carregarTurma() {
    authFetch(`${API_BASE}/turmas/${id}`)
        .then(res => res.json())
        .then(turma => {
            nomeTurmaInput.value = turma.nome;
            renderizarAlunos(turma.alunos);
        });
}

function renderizarAlunos(alunos) {
    listaAlunos.innerHTML = "";

    if (alunos.length === 0) {
        listaAlunos.innerHTML = "<p>Nenhum aluno matriculado ainda.</p>";
        return;
    }

    alunos.forEach(aluno => {
        const card = document.createElement("div");
        card.className = "question-card";

        card.innerHTML = `
            <h4>${aluno.nome}</h4>
            <p>${aluno.email}</p>
            <button type="button" onclick="removerAluno(${aluno.idUsuario})" class="btn btn-danger">Remover da turma</button>
        `;

        listaAlunos.appendChild(card);
    });
}

function carregarAlunosDisponiveis() {
    authFetch(`${API_BASE}/turmas/alunos`)
        .then(res => res.json())
        .then(alunos => {
            selecionarAlunoSelect.innerHTML = "";
            alunos.forEach(aluno => {
                const option = document.createElement("option");
                option.value = aluno.idUsuario;
                option.textContent = aluno.turmaNome
                    ? `${aluno.nome} (atualmente em ${aluno.turmaNome})`
                    : aluno.nome;
                selecionarAlunoSelect.appendChild(option);
            });
        });
}

function salvarNome() {
    const nome = nomeTurmaInput.value.trim();
    if (!nome) {
        alert("Informe o nome da turma.");
        return;
    }

    authFetch(`${API_BASE}/turmas/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ nome })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro ao salvar");
            }
            alert("Nome atualizado.");
        })
        .catch(() => alert("Erro ao salvar o nome da turma."));
}

function matricularAluno() {
    const alunoId = Number(selecionarAlunoSelect.value);
    if (!alunoId) {
        alert("Selecione um aluno.");
        return;
    }

    authFetch(`${API_BASE}/turmas/${id}/alunos`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ alunoId })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro ao matricular");
            }
            carregarTurma();
            carregarAlunosDisponiveis();
        })
        .catch(() => alert("Erro ao matricular aluno."));
}

function removerAluno(alunoId) {
    if (!confirm("Remover esse aluno da turma?")) return;

    authFetch(`${API_BASE}/turmas/${id}/alunos/${alunoId}`, {
        method: "DELETE"
    })
        .then(() => {
            carregarTurma();
            carregarAlunosDisponiveis();
        });
}
