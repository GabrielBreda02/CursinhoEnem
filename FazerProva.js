// ===============================
// Script da Página de Realização de Prova (aluno) — timer + respostas + redação
// ===============================

const params = new URLSearchParams(window.location.search);
const provaId = params.get("provaId");

const timerBadge = document.getElementById("timerBadge");
const tituloProva = document.getElementById("tituloProva");
const questoesContainer = document.getElementById("questoesContainer");
const secaoRedacao = document.getElementById("secaoRedacao");
const temaRedacaoTituloEl = document.getElementById("temaRedacaoTitulo");
const temaRedacaoTextoEl = document.getElementById("temaRedacaoTexto");
const textoRedacaoInput = document.getElementById("textoRedacao");

let tentativaId = null;
let expiraEm = null;
let intervalId = null;
let jaFinalizando = false;

if (!provaId) {
    tituloProva.textContent = "Prova não encontrada.";
} else {
    authFetch(`${API_BASE}/tentativas/iniciar`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ provaId: Number(provaId) })
    })
        .then(async response => {
            const dados = await response.json().catch(() => ({}));
            if (!response.ok) {
                throw new Error(dados.message || "Não foi possível iniciar a prova.");
            }
            return dados;
        })
        .then(iniciarTela)
        .catch(erro => {
            tituloProva.textContent = "Erro ao iniciar a prova";
            questoesContainer.innerHTML = `<p>${erro.message}</p>`;
        });
}

function iniciarTela(dados) {
    tentativaId = dados.idTentativa;
    expiraEm = new Date(dados.expiraEm);
    tituloProva.textContent = dados.provaTitulo;

    questoesContainer.innerHTML = "";
    dados.questoes.forEach((questao, index) => {
        questoesContainer.appendChild(criarCardQuestao(questao, index));
    });

    if (dados.temaRedacaoTitulo) {
        secaoRedacao.style.display = "block";
        temaRedacaoTituloEl.textContent = dados.temaRedacaoTitulo;
        temaRedacaoTextoEl.textContent = dados.temaRedacaoTexto;
    }

    atualizarTimer();
    intervalId = setInterval(atualizarTimer, 1000);
}

function criarCardQuestao(questao, index) {
    const card = document.createElement("div");
    card.className = "question-card";

    const imagemHtml = questao.imagemUrl
        ? `<img src="${API_BASE.replace(/\/api$/, "")}${questao.imagemUrl}" style="max-width:100%;border-radius:8px;margin:8px 0;">`
        : "";

    const alternativasHtml = questao.alternativas.map(alt => `
        <div class="alternativa">
            <input type="radio" name="questao-${questao.idQuestao}" id="alt-${alt.idAlternativa}" value="${alt.idAlternativa}">
            <label for="alt-${alt.idAlternativa}" style="font-weight:normal;margin:0;">${alt.descricao}</label>
        </div>
    `).join("");

    card.innerHTML = `
        <h4>${index + 1}. ${questao.titulo}</h4>
        ${imagemHtml}
        <p style="font-size:0.85rem;color:#777;">${questao.area}</p>
        ${alternativasHtml}
    `;

    card.querySelectorAll(`input[name="questao-${questao.idQuestao}"]`).forEach(radio => {
        radio.addEventListener("change", () => salvarResposta(questao.idQuestao, Number(radio.value)));
    });

    return card;
}

function salvarResposta(questaoId, alternativaId) {
    authFetch(`${API_BASE}/tentativas/${tentativaId}/respostas`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ questaoId, alternativaId })
    }).catch(erro => console.error("Erro ao salvar resposta:", erro));
}

function atualizarTimer() {
    const restanteMs = expiraEm - new Date();

    if (restanteMs <= 0) {
        timerBadge.textContent = "00:00:00";
        clearInterval(intervalId);
        alert("O tempo acabou! Sua prova será finalizada automaticamente.");
        finalizarProva(true);
        return;
    }

    const totalSegundos = Math.floor(restanteMs / 1000);
    const horas = Math.floor(totalSegundos / 3600);
    const minutos = Math.floor((totalSegundos % 3600) / 60);
    const segundos = totalSegundos % 60;
    timerBadge.textContent = [horas, minutos, segundos].map(n => String(n).padStart(2, "0")).join(":");

    if (totalSegundos <= 300) {
        timerBadge.classList.add("timer-alerta");
    }
}

function finalizarProva(automatico) {
    if (jaFinalizando) return;

    if (!automatico && !confirm("Deseja finalizar a prova agora? Depois de finalizada não é possível alterar as respostas.")) {
        return;
    }

    jaFinalizando = true;
    clearInterval(intervalId);

    authFetch(`${API_BASE}/tentativas/${tentativaId}/finalizar`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ textoRedacao: textoRedacaoInput.value || null })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro ao finalizar a prova");
            }
            window.location.href = `ResultadoProva.html?id=${tentativaId}`;
        })
        .catch(erro => {
            console.error(erro);
            alert("Erro ao finalizar a prova. Tente novamente.");
            jaFinalizando = false;
        });
}
