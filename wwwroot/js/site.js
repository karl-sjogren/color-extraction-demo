﻿var controller;
var colorPicker;


function loadImages(search) {
    if(controller) {
        controller.abort();
    }

    controller = new AbortController();

    const container = document.getElementById('image-list');
    fetch('/color/all?hexColor=' + (search || ''), { signal: controller.signal })
        .then(response => response.json())
        .then(images => {
            while(!!container.firstChild) {
                container.removeChild(container.lastChild);
            }

            images.sort((a, b) => {
                const higestFractionA = Math.max(...a.colors.filter(x => x.matchedColor).map(x => x.fraction));
                const higestFractionB = Math.max(...b.colors.filter(x => x.matchedColor).map(x => x.fraction));

                if(higestFractionA > higestFractionB) {
                    return -1;
                } else if(higestFractionA < higestFractionB) {
                    return 1;
                }
                return 0;
            });

            for(const image of images) {
                const item = document.createElement('li');

                const img = document.createElement('img');
                img.setAttribute('src', '/images/' + image.filename);
                item.appendChild(img);

                const span = document.createElement('span');
                span.textContent = image.filename;
                item.appendChild(span);

                const colorContainer = document.createElement('div');
                item.appendChild(colorContainer);

                const totalScore = image.colors.reduce((acc, val) => acc + val.fraction, 0);

                for(const color of image.colors) {
                    const colorBlock = document.createElement('div');
                    colorBlock.style.backgroundColor = '#' + color.hex;
                    colorBlock.style.width = (color.fraction / totalScore) * 100 + '%';
                    colorBlock.style.borderBottom = color.matchedColor ? '4px solid #000' : '';
                    colorBlock.addEventListener('click', () => {
                        if(!colorPicker)
                            return;
                        
                        colorPicker.color.hexString = '#' + color.hex;
                    })
                    colorContainer.appendChild(colorBlock);
                }
                container.appendChild(item);
            }
        });
}

document.addEventListener('DOMContentLoaded', () => {
    loadImages();
    
    colorPicker = new iro.ColorPicker('#picker');
    colorPicker.on('color:change', function(color) {
        loadImages(color.hexString.substring(1));
      });
});