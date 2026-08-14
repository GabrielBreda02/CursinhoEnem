// ===============================
// Script da Página de Listagem de Temas de Redação
// ===============================

const container = document.getElementById("listaTemas");

fetch(`${API_BASE}/temas-redacao`)
    .then(response => response.json())
    .then(temas => {
        container.innerHTML = "";

        if (temas.length === 0) {
            container.innerHTML = "<p>Nenhum tema de redação cadastrado.</p>";
            return;
        }

        temas.forEach(tema => {
            const card = document.createElement("div");
            card.className = "question-card";

            const fonteHtml = tema.fonte
                ? `<p><strong>Fonte:</strong> ${tema.fonte}${tema.ano ? " (" + tema.ano + ")" : ""}</p>`
                : "";

            card.innerHTML = `
                <h3>${tema.titulo}</h3>
                ${fonteHtml}
                <div class="acoes">
                    <a class="btn" href="TemaRedacao.html?id=${tema.idTemaRedacao}">Editar</a>
                    <button type="button" onclick="excluirTema(${tema.idTemaRedacao})" class="btn btn-danger"> Excluir</button>
                </div>
            `;

            container.appendChild(card);
        });
    })
    .catch(() => {
        container.innerHTML = "<p>Erro ao carregar os temas de redação.</p>";
    });

function excluirTema(idTema) {
    if (!idTema) return;

    if (confirm("Deseja excluir esse tema de redação?")) {
        authFetch(`${API_BASE}/temas-redacao/${idTema}`, {
            method: "DELETE"
        }).then(() => window.location.href = "TemasRedacao.html");
    }
}
