import { useState } from 'react';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import SimpleResourcePage from './SimpleResourcePage';

export default function DishesManagementScreen() {
  const [tab, setTab] = useState<'categories' | 'dishes' | 'placeDishes'>('categories');
  return <div>
    <div className="mb-4 flex gap-2">
      <Button variant={tab === 'categories' ? 'primary' : 'secondary'} onClick={() => setTab('categories')}>Dish Categories</Button>
      <Button variant={tab === 'dishes' ? 'primary' : 'secondary'} onClick={() => setTab('dishes')}>Dishes</Button>
      <Button variant={tab === 'placeDishes' ? 'primary' : 'secondary'} onClick={() => setTab('placeDishes')}>Place Dishes</Button>
    </div>
    {tab === 'categories' && <SimpleResourcePage config={{ title: 'Dish Categories', endpoint: endpoints.dishCategories, fields: [
      { name: 'categoryName', label: 'Category Name' }, { name: 'description', label: 'Description', type: 'textarea' }, { name: 'isActive', label: 'Active', type: 'checkbox' }
    ], columns: [ { key: 'categoryId', label: 'Id' }, { key: 'categoryName', label: 'Name' }, { key: 'description', label: 'Description' }, { key: 'isActive', label: 'Active', render: r => r.isActive ? 'Yes' : 'No' } ] }} />}
    {tab === 'dishes' && <SimpleResourcePage config={{ title: 'Dishes', endpoint: endpoints.dishes, fields: [
      { name: 'dishName', label: 'Dish Name' }, { name: 'categoryId', label: 'Category Id', type: 'number' }, { name: 'description', label: 'Description', type: 'textarea' }, { name: 'imageUrl', label: 'Image URL' }, { name: 'averagePrice', label: 'Average Price', type: 'number' }, { name: 'isSignatureDish', label: 'Signature Dish', type: 'checkbox' }, { name: 'isActive', label: 'Active', type: 'checkbox' }
    ], columns: [ { key: 'dishId', label: 'Id' }, { key: 'dishName', label: 'Name' }, { key: 'categoryId', label: 'Category' }, { key: 'averagePrice', label: 'Price' }, { key: 'isSignatureDish', label: 'Signature', render: r => r.isSignatureDish ? 'Yes' : 'No' } ] }} />}
    {tab === 'placeDishes' && <SimpleResourcePage config={{ title: 'Place Dishes', endpoint: endpoints.placeDishes, softDelete: false, fields: [
      { name: 'placeId', label: 'Place Id', type: 'number' }, { name: 'dishId', label: 'Dish Id', type: 'number' }, { name: 'price', label: 'Price', type: 'number' }, { name: 'isRecommended', label: 'Recommended', type: 'checkbox' }, { name: 'note', label: 'Note', type: 'textarea' }
    ], columns: [ { key: 'placeDishId', label: 'Id' }, { key: 'placeId', label: 'Place' }, { key: 'dishId', label: 'Dish' }, { key: 'price', label: 'Price' }, { key: 'isRecommended', label: 'Recommended', render: r => r.isRecommended ? 'Yes' : 'No' } ] }} />}
  </div>;
}
