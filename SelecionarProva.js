// ===============================
// Script da Página de Seleção de Prova (aluno)
// ===============================

const container = document.getElementById("listaProvas");

fetch(`${API_BASE}/Provas`)
    .then(response => response.json())
    .then(provas => {
        container.innerHTML = "";

        if (provas.length === 0) {
            container.innerHTML = "<p>Nenhuma prova disponível ainda.</p>";
            return;
        }

        provas.forEach(prova => {
            const card = document.createElement("div");
            card.className = "question-card";

            const temaHtml = prova.temaRedacaoTitulo
                ? `<p><strong>Redação:</strong> ${prova.temaRedacaoTitulo}</p>`
                : "";
            const turmaHtml = prova.turma
                ? `<p><strong>Turma:</strong> ${prova.turma}</p>`
                : "";

            card.innerHTML = `
                <h3>${prova.titulo}</h3>
                ${turmaHtml}
                <p><strong>Questões:</strong> ${prova.quantidadeQuestoes} — <strong>Tempo:</strong> ${prova.tempoLimiteMinutos} minutos</p>
                ${temaHtml}
                <div class="acoes">
                    <button type="button" class="btn" onclick="iniciarProva(${prova.idProva})">Iniciar</button>
                </div>
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar as provas.</p>";
    });

function iniciarProva(idProva) {
    if (!confirm("Ao iniciar, o cronômetro começa a contar e não pode ser pausado. Deseja continuar?")) {
        return;
    }
    window.location.href = `FazerProva.html?provaId=${idProva}`;
}
