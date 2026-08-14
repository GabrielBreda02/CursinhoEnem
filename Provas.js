const container = document.getElementById("tabelaProvas");

fetch(`${API_BASE}/Provas`)
    .then(response => response.json())
    .then(provas => {
        container.innerHTML = "";

        if (provas.length === 0) {
            container.innerHTML = "<p>Nenhuma prova cadastrada.</p>";
            return;
        }

        provas.forEach(prova => {
            const card = document.createElement("div");
            card.className = "question-card";

            const temaHtml = prova.temaRedacaoTitulo
                ? `<p><strong>Tema de redação:</strong> ${prova.temaRedacaoTitulo}</p>`
                : "";

            card.innerHTML = `
                <h3>${prova.titulo}</h3>
                <p><strong>Disciplina:</strong> ${prova.disciplina}</p>
                <p><strong>Quantidade de questões:</strong> ${prova.quantidadeQuestoes}</p>
                <p><strong>Tempo limite:</strong> ${prova.tempoLimiteMinutos} minutos</p>
                ${temaHtml}
                <div class="acoes">
                    <a class="btn" href="prova.html?id=${prova.idProva}">Editar</a>
                    <button type="button" onclick="excluirProva(${prova.idProva})" class="btn btn-danger"> Excluir</button>
                </div>
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar as provas.</p>";
    });
function excluirProva(id) {
    if (!id) return;

    if (confirm("Deseja excluir essa prova?")) {
        authFetch(`${API_BASE}/Provas/${id}`, {
            method: "DELETE"
        }).then(() => window.location.href = "Provas.html");
    }
}