// Pipeline drag and drop functionality

document.addEventListener('DOMContentLoaded', function() {
    const dealCards = document.querySelectorAll('.deal-card');
    const columns = document.querySelectorAll('.pipeline-column');
    
    // Make deal cards draggable
    dealCards.forEach(card => {
        card.draggable = true;
        card.addEventListener('dragstart', handleDragStart);
        card.addEventListener('dragend', handleDragEnd);
    });
    
    // Make columns drop zones
    columns.forEach(column => {
        column.addEventListener('dragover', handleDragOver);
        column.addEventListener('drop', handleDrop);
        column.addEventListener('dragenter', handleDragEnter);
        column.addEventListener('dragleave', handleDragLeave);
    });
    
    let draggedElement = null;
    
    function handleDragStart(e) {
        draggedElement = this;
        this.style.opacity = '0.5';
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }
    
    function handleDragEnd(e) {
        this.style.opacity = '1';
        columns.forEach(col => col.classList.remove('drag-over'));
    }
    
    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        return false;
    }
    
    function handleDragEnter(e) {
        this.classList.add('drag-over');
    }
    
    function handleDragLeave(e) {
        this.classList.remove('drag-over');
    }
    
    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }
        
        if (draggedElement !== this) {
            // Get the deal ID and new stage from the column
            const dealId = draggedElement.dataset.dealId;
            const newStage = this.dataset.stage;
            
            if (dealId && newStage) {
                // Submit form to update deal stage
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = `/deals/${dealId}/stage`;
                
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = 'stage';
                input.value = newStage;
                
                form.appendChild(input);
                document.body.appendChild(form);
                form.submit();
            }
        }
        
        this.classList.remove('drag-over');
        return false;
    }
});

