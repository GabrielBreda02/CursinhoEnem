// ===============================
// Script da Página de Turmas (professor)
// ===============================

const container = document.getElementById("listaTurmas");
const nomeTurmaInput = document.getElementById("nomeTurma");

function carregarTurmas() {
    authFetch(`${API_BASE}/turmas`)
        .then(res => res.json())
        .then(turmas => {
            container.innerHTML = "";

            if (turmas.length === 0) {
                container.innerHTML = "<p>Nenhuma turma cadastrada ainda.</p>";
                return;
            }

            turmas.forEach(turma => {
                const card = document.createElement("div");
                card.className = "question-card";

                card.innerHTML = `
                    <h3>${turma.nome}</h3>
                    <p><strong>Alunos matriculados:</strong> ${turma.quantidadeAlunos}</p>
                    <div class="acoes">
                        <a class="btn" href="Turma.html?id=${turma.idTurma}">Gerenciar</a>
                        <button type="button" onclick="excluirTurma(${turma.idTurma})" class="btn btn-danger">Excluir</button>
                    </div>
                `;

                container.appendChild(card);
            });
        })
        .catch(() => {
            container.innerHTML = "<p>Erro ao carregar as turmas.</p>";
        });
}

carregarTurmas();

function criarTurma() {
    const nome = nomeTurmaInput.value.trim();
    if (!nome) {
        alert("Informe o nome da turma.");
        return;
    }

    authFetch(`${API_BASE}/turmas`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ nome })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro ao criar turma");
            }
            nomeTurmaInput.value = "";
            carregarTurmas();
        })
        .catch(erro => {
            console.error(erro);
            alert("Erro ao criar turma.");
        });
}

function excluirTurma(id) {
    if (confirm("Deseja excluir essa turma? Os alunos matriculados nela ficam sem turma (não são excluídos).")) {
        authFetch(`${API_BASE}/turmas/${id}`, {
            method: "DELETE"
        }).then(() => carregarTurmas());
    }
}
