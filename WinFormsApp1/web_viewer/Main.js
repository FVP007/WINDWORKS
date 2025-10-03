const modelViewer = document.querySelector('#viewer');
const loadingDiv = document.querySelector('#loading');

// Mostra o viewer quando carregar
if (modelViewer) {
    modelViewer.addEventListener('load', () => {
        console.log('Model loaded successfully');
        if (loadingDiv) loadingDiv.style.display = 'none';
        modelViewer.style.display = 'block';
    });

    modelViewer.addEventListener('error', (event) => {
        console.error('Error loading model:', event);
        if (loadingDiv) {
            loadingDiv.textContent = 'Erro ao carregar modelo 3D';
            loadingDiv.style.color = 'red';
        }
    });
}

// Função para carregar modelo
window.loadModel = function (wingType) {
    console.log(`Attempting to load model: ${wingType}`);

    if (!modelViewer) {
        console.error('Model viewer not found');
        return;
    }

    // Caminho relativo ao index.html
    const modelPath = `models/${wingType}.glb`;
    console.log(`Full path: ${modelPath}`);

    // Mostra loading
    if (loadingDiv) {
        loadingDiv.style.display = 'block';
        loadingDiv.textContent = `Carregando ${wingType}...`;
        loadingDiv.style.color = 'white';
    }

    modelViewer.src = modelPath;
    modelViewer.style.display = 'block';
};

// Função para mudar câmera
window.changeCameraView = function (viewName) {
    if (!modelViewer) {
        console.error('Model viewer not found');
        return;
    }

    let orbit;
    switch (viewName) {
        case "Top View":
            orbit = "0deg 0deg 150%";
            break;
        case "Front View":
            orbit = "0deg 90deg 150%";
            break;
        case "Side View":
            orbit = "90deg 90deg 150%";
            break;
        case "Isometric View":
        default:
            orbit = "45deg 75deg 105%";
            break;
    }

    modelViewer.cameraOrbit = orbit;
    console.log(`Camera changed to: ${viewName} (${orbit})`);
};

console.log('main.js loaded successfully');