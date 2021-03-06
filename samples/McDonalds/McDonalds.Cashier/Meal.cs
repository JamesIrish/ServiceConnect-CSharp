﻿using System;
using McDonalds.Messages;
using ServiceConnect.Core;
using ServiceConnect.Interfaces;

namespace McDonalds.Cashier
{
    public class Meal : ProcessManager<MealData>, IStartProcessManager<NewOrderMessage>,
                                                  IMessageHandler<BurgerCookedMessage>,
                                                  IMessageHandler<FoodPrepped>
    {
        private readonly IBus _bus;

        public Meal(IBus bus)
        {
            _bus = bus;
        }

        public void Execute(NewOrderMessage message)
        {
            Data.CorrelationId = Guid.NewGuid();
            Data.Meal = message.Name;
            Data.Size = message.Size;

            Console.WriteLine("New order recieved: Meal - {0}, Size - {1}, OrderId - {2}", message.Name, message.Size, message.CorrelationId);

            var prepFoodMessage = new PrepFoodMessage(Data.CorrelationId)
            {
                BunSize = message.Size
            };
            Console.WriteLine("Prepping meal");
            _bus.Publish(prepFoodMessage);

            var flipBurgerMessage = new CookBurgerMessage(Data.CorrelationId)
            {
                BurgerSize = message.Size
            };
            Console.WriteLine("Cooking burger");
            _bus.Publish(flipBurgerMessage);
        }

        public void Execute(BurgerCookedMessage message)
        {
            Console.WriteLine("Burger cooked for order {0}", message.CorrelationId);

            Data.BurgerCooked = true;
            if (Data.FoodPrepped)
            {
                _bus.Publish(new OrderReadyMessage(message.CorrelationId)
                {
                    Size = Data.Size,
                    Meal = Data.Meal
                });
                Complete = true;
                Console.WriteLine("Order ready: OrderId - {0}", message.CorrelationId);
            }
        }

        public void Execute(FoodPrepped message)
        {
            Console.WriteLine("Food prepped for order {0}", message.CorrelationId);

            Data.FoodPrepped = true;
            if (Data.BurgerCooked)
            {
                _bus.Publish(new OrderReadyMessage(message.CorrelationId)
                {
                    Size = Data.Size,
                    Meal = Data.Meal
                });
                Complete = true;
                Console.WriteLine("Order ready: OrderId - {0}", message.CorrelationId);
            }
        }
    }
}