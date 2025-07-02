#!/bin/bash

# Cleanup script for redundant test projects
echo "Cleaning up redundant test projects..."

# Create a backup directory
mkdir -p backup_tests

# Move the redundant test projects to the backup directory
echo "Moving redundant test projects to backup directory..."
mv "Appointment System/Appointment.System.Tests" backup_tests/ 2>/dev/null
mv "SimpleTests" backup_tests/ 2>/dev/null
mv "SimpleTestsDemo" backup_tests/ 2>/dev/null
mv "TestConsole" backup_tests/ 2>/dev/null

# Also remove the test directories from within the main project
echo "Removing test directories from within the main project..."
rm -rf "Appointment System/SimpleTests" 2>/dev/null
rm -rf "Appointment System/SimpleTestsDemo" 2>/dev/null
rm -rf "Appointment System/TestConsole" 2>/dev/null

echo "Cleaning build artifacts..."
dotnet clean

echo "Cleanup complete. Redundant test projects have been moved to the backup_tests directory."
echo "The main test project is now AppointmentSystem.Tests."
echo ""
echo "To run the tests, use: dotnet test AppointmentSystem.Tests" 