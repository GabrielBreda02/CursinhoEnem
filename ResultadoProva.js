// ===============================
// Script da Página de Resultado da Prova (aluno)
// ===============================

const params = new URLSearchParams(window.location.search);
const tentativaId = params.get("id");

const tituloProva = document.getElementById("tituloProva");
const placarNumero = document.getElementById("placarNumero");
const placarLegenda = document.getElementById("placarLegenda");
const questoesResultado = document.getElementById("questoesResultado");
const secaoRedacaoResultado = document.getElementById("secaoRedacaoResultado");
const temaRedacaoResultado = document.getElementById("temaRedacaoResultado");
const textoRedacaoResultado = document.getElementById("textoRedacaoResultado");

if (!tentativaId) {
    tituloProva.textContent = "Resultado não encontrado.";
} else {
    authFetch(`${API_BASE}/tentativas/${tentativaId}`)
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível carregar o resultado.");
            }
            return dados;
        })
        .then(renderizarResultado)
        .catch(erro => {
            tituloProva.textContent = "Erro ao carregar resultado";
            questoesResultado.innerHTML = `<p>${erro.message}</p>`;
        });
}

function renderizarResultado(dados) {
    tituloProva.textContent = dados.provaTitulo;
    placarNumero.textContent = `${dados.notaObjetivas} / ${dados.totalQuestoes}`;
    placarLegenda.textContent = "questões objetivas corretas";

    questoesResultado.innerHTML = "";
    dados.questoes.forEach((questao, index) => {
        const card = document.createElement("div");
        card.className = "question-card";

        const imagemHtml = questao.imagemUrl
            ? `<img src="${API_BASE.replace(/\/api$/, "")}${questao.imagemUrl}" style="max-width:100%;border-radius:8px;margin:8px 0;">`
            : "";

        const alternativasHtml = questao.alternativas.map(alt => {
            let classe = "";
            if (alt.correta) {
                classe = "alternativa-correta";
            } else if (alt.idAlternativa === questao.alternativaSelecionadaId) {
                classe = "alternativa-selecionada-errada";
            }
            const marcador = alt.idAlternativa === questao.alternativaSelecionadaId ? "→ " : "";
            return `<p class="${classe}">${marcador}${alt.descricao}</p>`;
        }).join("");

        const statusHtml = questao.respondidaCorretamente
            ? '<span class="status-correta">(Correta)</span>'
            : '<span class="status-incorreta">(Incorreta)</span>';

        card.innerHTML = `
            <h4>${index + 1}. ${questao.titulo} ${statusHtml}</h4>
            ${imagemHtml}
            ${alternativasHtml}
        `;

        questoesResultado.appendChild(card);
    });

    if (dados.temaRedacaoTitulo) {
        secaoRedacaoResultado.style.display = "block";
        temaRedacaoResultado.textContent = dados.temaRedacaoTitulo;
        textoRedacaoResultado.textContent = dados.textoRedacao || "(nenhum texto enviado)";
    }
}
