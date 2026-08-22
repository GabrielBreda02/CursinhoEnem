// ===============================
// Autenticação - funções compartilhadas por todas as páginas
// ===============================

const API_BASE = "http://localhost:5000/api";

const TOKEN_KEY = "bq_token";
const NOME_KEY = "bq_nome";
const EMAIL_KEY = "bq_email";
const TIPO_KEY = "bq_tipo";

function salvarSessao(token, nome, email, tipo) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(NOME_KEY, nome);
    localStorage.setItem(EMAIL_KEY, email);
    localStorage.setItem(TIPO_KEY, tipo);
}

function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function getNomeUsuario() {
    return localStorage.getItem(NOME_KEY);
}

function getTipoUsuario() {
    return localStorage.getItem(TIPO_KEY);
}

function ehProfessor() {
    return getTipoUsuario() === "Professor";
}

function ehAluno() {
    return getTipoUsuario() === "Aluno";
}

function estaLogado() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(NOME_KEY);
    localStorage.removeItem(EMAIL_KEY);
    localStorage.removeItem(TIPO_KEY);
    window.location.href = "Login.html";
}

// fetch com o header Authorization já preenchido, para ações que exigem login.
// Se a API responder 401 (token ausente/expirado), limpa a sessão e manda para o login.
function authFetch(url, options = {}) {
    const token = getToken();
    const headers = { ...(options.headers || {}) };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    return fetch(url, { ...options, headers }).then(response => {
        if (response.status === 401) {
            alert("Sua sessão expirou ou você não está logado. Faça login novamente.");
            logout();
            throw new Error("Não autenticado");
        }
        return response;
    });
}

// Nomes oficiais do ENEM (Questao.area, valor salvo no banco) são longos demais para
// exibição ("Matemática e suas Tecnologias") — isso encurta só o texto mostrado na tela.
function formatArea(area) {
    return (area || "").replace(/\s+e\s+suas\s+Tecnologias\s*$/i, "").trim();
}

// Monta os botões de paginação (‹ Anterior, números com reticências, Próxima ›) e chama
// aoMudarPagina(pagina) quando um deles é clicado. Usado em Questoes.js e Prova.js.
function criarControlesPaginacao(paginaAtual, totalPaginas, aoMudarPagina) {
    const nav = document.createElement("div");
    nav.className = "paginacao";

    if (totalPaginas <= 1) {
        return nav;
    }

    function criarBotao(texto, pagina, { ativo = false, desabilitado = false } = {}) {
        const btn = document.createElement("button");
        btn.type = "button";
        btn.textContent = texto;
        btn.className = ativo ? "btn" : "btn btn-ghost";
        btn.disabled = desabilitado;
        if (!desabilitado) {
            btn.addEventListener("click", () => aoMudarPagina(pagina));
        }
        return btn;
    }

    nav.appendChild(criarBotao("‹ Anterior", paginaAtual - 1, { desabilitado: paginaAtual <= 1 }));

    const paginasAMostrar = new Set([1, totalPaginas, paginaAtual - 1, paginaAtual, paginaAtual + 1]);
    let ultimaExibida = 0;
    Array.from(paginasAMostrar)
        .filter(p => p >= 1 && p <= totalPaginas)
        .sort((a, b) => a - b)
        .forEach(p => {
            if (ultimaExibida && p - ultimaExibida > 1) {
                const reticencias = document.createElement("span");
                reticencias.className = "paginacao-reticencias";
                reticencias.textContent = "…";
                nav.appendChild(reticencias);
            }
            nav.appendChild(criarBotao(String(p), p, { ativo: p === paginaAtual }));
            ultimaExibida = p;
        });

    nav.appendChild(criarBotao("Próxima ›", paginaAtual + 1, { desabilitado: paginaAtual >= totalPaginas }));

    return nav;
}

// Preenche a barra de navegação (marca + usuário/sair, ou link de login) em qualquer
// página que tenha um <div id="navbar"></div>. Chamado automaticamente ao carregar a página.
function renderNavbar() {
    const el = document.getElementById("navbar");
    if (!el) return;

    el.innerHTML = estaLogado()
        ? `<div class="navbar">
             <a href="index.html" class="navbar-brand">CursinhoEnem</a>
             <div class="navbar-user">
               <span><strong>${getNomeUsuario()}</strong><span class="badge-tipo">${getTipoUsuario()}</span></span>
               <button class="btn btn-ghost" onclick="logout()">Sair</button>
             </div>
           </div>`
        : `<div class="navbar">
             <a href="index.html" class="navbar-brand">CursinhoEnem</a>
             <div class="navbar-user">
               <a href="Login.html" class="btn btn-ghost">Entrar</a>
             </div>
           </div>`;
}

document.addEventListener("DOMContentLoaded", renderNavbar);
