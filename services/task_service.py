from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date
from db.models.task import Task, TaskStatus, TaskPriority
from schema.task import TaskCreate, TaskUpdate


def get_tasks(
    db: Session,
    skip: int = 0,
    limit: int = 50,
    assigned_to: Optional[int] = None,
    status: Optional[TaskStatus] = None,
    priority: Optional[TaskPriority] = None
) -> tuple[List[Task], int]:
    """Get tasks with pagination and filters"""
    query = db.query(Task)
    
    if assigned_to:
        query = query.filter(Task.assigned_to == assigned_to)
    
    if status:
        query = query.filter(Task.status == status)
    
    if priority:
        query = query.filter(Task.priority == priority)
    
    total = query.count()
    tasks = query.order_by(Task.due_date.asc(), Task.created_at.desc()).offset(skip).limit(limit).all()
    
    return tasks, total


def get_task_by_id(db: Session, task_id: int) -> Optional[Task]:
    """Get task by ID"""
    return db.query(Task).filter(Task.id == task_id).first()


def create_task(db: Session, task_data: TaskCreate, created_by: int) -> Task:
    """Create a new task"""
    db_task = Task(
        **task_data.model_dump(),
        created_by=created_by
    )
    db.add(db_task)
    db.commit()
    db.refresh(db_task)
    return db_task


def update_task(db: Session, task_id: int, task_data: TaskUpdate) -> Optional[Task]:
    """Update a task"""
    db_task = get_task_by_id(db, task_id)
    if not db_task:
        return None
    
    update_data = task_data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(db_task, field, value)
    
    db.commit()
    db.refresh(db_task)
    return db_task


def complete_task(db: Session, task_id: int) -> Optional[Task]:
    """Mark task as completed"""
    db_task = get_task_by_id(db, task_id)
    if not db_task:
        return None
    
    db_task.status = TaskStatus.COMPLETED
    from datetime import datetime
    db_task.completed_at = datetime.now()
    
    db.commit()
    db.refresh(db_task)
    return db_task


def delete_task(db: Session, task_id: int) -> bool:
    """Delete a task"""
    db_task = get_task_by_id(db, task_id)
    if not db_task:
        return False
    
    db.delete(db_task)
    db.commit()
    return True

